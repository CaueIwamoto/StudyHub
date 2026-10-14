using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Mappings;
using StudyHub.Application.Validation;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Exceptions;
using StudyHub.Domain.Interfaces;

namespace StudyHub.Application.Services;

public sealed class PropostaService : IPropostaService
{
    private const decimal ValorMaximo = 9_999_999_999_999_999.99m; // limite de numeric(18,2)

    private readonly IPropostaRepository _propostas;
    private readonly IPedidoRepository _pedidos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public PropostaService(
        IPropostaRepository propostas,
        IPedidoRepository pedidos,
        IUsuarioRepository usuarios,
        IUnitOfWork unitOfWork)
    {
        _propostas = propostas;
        _pedidos = pedidos;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<PropostaResponse> CriarAsync(Guid prestadorId, Guid pedidoId, CriarPropostaRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);
        var (tipo, valor) = ValidarRequest(request);

        // Regra 1: apenas Prestador envia propostas (verificado no banco, além do [Authorize(Roles)] da API).
        var prestador = await _usuarios.GetByIdAsync(prestadorId, cancellationToken)
            ?? throw new UnauthorizedException("Usuário do token não encontrado.");
        if (prestador.Tipo != TipoUsuario.Prestador)
            throw new ForbiddenException("Apenas usuários do tipo Prestador podem enviar propostas.");

        var mensagem = string.IsNullOrWhiteSpace(request.Mensagem) ? null : request.Mensagem.Trim();

        var proposta = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Bloqueia o pedido: serializa envio de propostas com aceite/recusa/cancelamento concorrentes.
            var pedido = await _pedidos.GetByIdParaAtualizacaoAsync(pedidoId, ct)
                ?? throw new NotFoundException("Pedido não encontrado.");

            // Regra 3: prestador não envia proposta para pedido próprio.
            if (pedido.PertenceA(prestador.Id))
                throw new ForbiddenException("Você não pode enviar proposta para o seu próprio pedido.");

            // Regra 9: pedidos Fechados ou Cancelados não recebem propostas.
            if (!pedido.EstaDisponivel)
                throw new DomainException($"Pedidos com status {pedido.Status} não aceitam novas propostas.");

            // Regras 7 e 8: coerência entre o tipo e o valor inicial do pedido.
            var erros = new ValidationErrors();
            if (tipo == TipoProposta.AceiteValorInicial && valor != pedido.ValorInicial)
                erros.Add(nameof(CriarPropostaRequest.ValorProposto),
                    "Na proposta do tipo AceiteValorInicial, o valor deve ser igual ao valor inicial do pedido.");
            if (tipo == TipoProposta.Contraproposta && valor == pedido.ValorInicial)
                erros.Add(nameof(CriarPropostaRequest.ValorProposto),
                    "Na proposta do tipo Contraproposta, o valor deve ser diferente do valor inicial do pedido.");
            erros.ThrowIfAny();

            // Regra 5: somente uma proposta Pendente por prestador e pedido (o índice único parcial é a garantia final).
            if (await _propostas.ExistePendenteDoPrestadorAsync(pedido.Id, prestador.Id, ct))
                throw new ConflictException("Você já possui uma proposta pendente para este pedido.");

            var nova = Proposta.Criar(pedido.Id, prestador.Id, valor, mensagem, tipo); // PrestadorId vem do JWT
            await _propostas.AddAsync(nova, ct);
            pedido.RegistrarProposta(); // Aberto -> EmNegociacao
            return nova;
        }, cancellationToken);

        return proposta.ToResponse();
    }

    public async Task<IReadOnlyList<PropostaDetalhadaResponse>> ListarPorPedidoAsync(Guid clienteId, Guid pedidoId, StatusProposta? status, CancellationToken cancellationToken = default)
    {
        ValidarStatus(status);

        var pedido = await _pedidos.GetByIdAsync(pedidoId, cancellationToken)
            ?? throw new NotFoundException("Pedido não encontrado.");

        // Propostas recebidas são privadas: somente o cliente proprietário do pedido as consulta.
        if (!pedido.PertenceA(clienteId))
            throw new ForbiddenException("Apenas o proprietário do pedido pode consultar as propostas recebidas.");

        var propostas = await _propostas.ListarPorPedidoAsync(pedidoId, status, cancellationToken);
        return propostas.Select(p => p.ToDetalhadaResponse()).ToList();
    }

    public async Task<IReadOnlyList<PropostaDetalhadaResponse>> ListarMinhasAsync(Guid prestadorId, StatusProposta? status, CancellationToken cancellationToken = default)
    {
        ValidarStatus(status);

        var prestador = await _usuarios.GetByIdAsync(prestadorId, cancellationToken)
            ?? throw new UnauthorizedException("Usuário do token não encontrado.");
        if (prestador.Tipo != TipoUsuario.Prestador)
            throw new ForbiddenException("Apenas usuários do tipo Prestador possuem propostas enviadas.");

        var propostas = await _propostas.ListarPorPrestadorAsync(prestadorId, status, cancellationToken);
        return propostas.Select(p => p.ToDetalhadaResponse()).ToList();
    }

    public async Task<PropostaDetalhadaResponse> ObterAsync(Guid usuarioId, Guid propostaId, CancellationToken cancellationToken = default)
    {
        var proposta = await _propostas.GetByIdAsync(propostaId, cancellationToken)
            ?? throw new NotFoundException("Proposta não encontrada.");

        var ehPrestadorAutor = proposta.PertenceAoPrestador(usuarioId);
        var ehClienteDono = proposta.Pedido is not null && proposta.Pedido.PertenceA(usuarioId);
        if (!ehPrestadorAutor && !ehClienteDono)
            throw new ForbiddenException("Você não tem permissão para consultar esta proposta.");

        return proposta.ToDetalhadaResponse();
    }

    public async Task<PropostaDetalhadaResponse> AceitarAsync(Guid clienteId, Guid propostaId, CancellationToken cancellationToken = default)
    {
        var proposta = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var (pedido, alvo) = await CarregarComBloqueioAsync(clienteId, propostaId, ct);

            // Regra 10 (somente Pendente pode ser aceita) e regra 19 (um pedido só fecha uma vez):
            // as entidades rejeitam transições inválidas com DomainException (HTTP 409).
            alvo.Aceitar();
            pedido.Fechar(alvo.ValorProposto); // Regras 12 e 13: Fechado + ValorAcordado (ValorInicial preservado)

            // Regra 14: as demais propostas pendentes viram NaoSelecionada.
            var outras = await _propostas.ListarPendentesDoPedidoAsync(pedido.Id, alvo.Id, ct);
            foreach (var outra in outras)
                outra.MarcarComoNaoSelecionada();

            return alvo;
        }, cancellationToken);

        return proposta.ToDetalhadaResponse();
    }

    public async Task<PropostaDetalhadaResponse> RecusarAsync(Guid clienteId, Guid propostaId, CancellationToken cancellationToken = default)
    {
        var proposta = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var (pedido, alvo) = await CarregarComBloqueioAsync(clienteId, propostaId, ct);

            alvo.Recusar(); // Regra 11: proposta Aceita (ou qualquer não Pendente) não pode ser recusada.

            // Regra 15: ao recusar a última proposta pendente, o pedido volta para Aberto.
            var restamPendentes = await _propostas.ExisteOutraPendenteAsync(pedido.Id, alvo.Id, ct);
            if (!restamPendentes && pedido.Status == StatusPedido.EmNegociacao)
                pedido.Reabrir();

            return alvo;
        }, cancellationToken);

        return proposta.ToDetalhadaResponse();
    }

    /// <summary>
    /// Descobre o pedido da proposta, bloqueia a linha do pedido (FOR UPDATE) e só então carrega a proposta,
    /// garantindo dados atuais. Valida que o usuário é o cliente proprietário do pedido (regra 2).
    /// Deve ser chamado dentro de uma transação.
    /// </summary>
    private async Task<(Pedido Pedido, Proposta Proposta)> CarregarComBloqueioAsync(Guid clienteId, Guid propostaId, CancellationToken ct)
    {
        var pedidoId = await _propostas.ObterPedidoIdAsync(propostaId, ct)
            ?? throw new NotFoundException("Proposta não encontrada.");

        var pedido = await _pedidos.GetByIdParaAtualizacaoAsync(pedidoId, ct)
            ?? throw new NotFoundException("Pedido não encontrado.");

        if (!pedido.PertenceA(clienteId))
            throw new ForbiddenException("Apenas o cliente proprietário do pedido pode aceitar ou recusar propostas.");

        var proposta = await _propostas.GetByIdAsync(propostaId, ct)
            ?? throw new NotFoundException("Proposta não encontrada.");

        return (pedido, proposta);
    }

    /// <summary>Regra 6: ValorProposto deve ser maior que zero (considerando 2 casas decimais).</summary>
    private static (TipoProposta Tipo, decimal Valor) ValidarRequest(CriarPropostaRequest request)
    {
        var erros = new ValidationErrors();

        var valor = decimal.Round(request.ValorProposto, 2, MidpointRounding.AwayFromZero);
        if (valor <= 0)
            erros.Add(nameof(request.ValorProposto), "O valor proposto deve ser maior que zero.");
        else if (valor > ValorMaximo)
            erros.Add(nameof(request.ValorProposto), "O valor proposto excede o máximo permitido.");

        var tipo = request.Tipo ?? default;
        if (request.Tipo is null || !Enum.IsDefined(typeof(TipoProposta), tipo))
            erros.Add(nameof(request.Tipo), "Tipo de proposta inválido.");

        erros.ThrowIfAny();
        return (tipo, valor);
    }

    private static void ValidarStatus(StatusProposta? status)
    {
        if (status is not null && !Enum.IsDefined(typeof(StatusProposta), status.Value))
        {
            var erros = new ValidationErrors();
            erros.Add("status", "Status inválido.");
            erros.ThrowIfAny();
        }
    }
}
