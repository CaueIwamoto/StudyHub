using StudyHub.Application.DTOs;
using StudyHub.Application.Exceptions;
using StudyHub.Application.Mappings;
using StudyHub.Application.Validation;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Interfaces;

namespace StudyHub.Application.Services;

public sealed class PedidoService : IPedidoService
{
    private const string CategoriaPadrao = "Geral";
    private const decimal ValorMaximo = 9_999_999_999_999_999.99m; // limite de numeric(18,2)

    private readonly IPedidoRepository _pedidos;
    private readonly IPropostaRepository _propostas;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnitOfWork _unitOfWork;

    public PedidoService(IPedidoRepository pedidos, IPropostaRepository propostas, IUsuarioRepository usuarios, IUnitOfWork unitOfWork)
    {
        _pedidos = pedidos;
        _propostas = propostas;
        _usuarios = usuarios;
        _unitOfWork = unitOfWork;
    }

    public async Task<PedidoDto> CriarAsync(Guid usuarioId, CriarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);
        var valor = ValidarValor(request.ValorInicial);

        // Regra 1: apenas Cliente cria pedidos (verificado no banco, além do [Authorize(Roles)] da API).
        var usuario = await _usuarios.GetByIdAsync(usuarioId, cancellationToken)
            ?? throw new UnauthorizedException("Usuário do token não encontrado.");
        if (usuario.Tipo != TipoUsuario.Cliente)
            throw new ForbiddenException("Apenas usuários do tipo Cliente podem criar pedidos.");

        var pedido = Pedido.Criar(
            usuario.Id, // Regra 2: ClienteId vem do JWT, nunca do corpo da requisição
            request.Titulo.Trim(),
            request.Descricao.Trim(),
            NormalizarCategoria(request.Categoria),
            valor);

        await _pedidos.AddAsync(pedido, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return pedido.ToDto();
    }

    public Task<PagedResult<PedidoDto>> ListarAsync(PedidoFiltroRequest filtro, CancellationToken cancellationToken = default)
        => BuscarAsync(filtro, null, false, cancellationToken);

    public Task<PagedResult<PedidoDto>> ListarDisponiveisAsync(PedidoFiltroRequest filtro, CancellationToken cancellationToken = default)
        => BuscarAsync(filtro, null, true, cancellationToken);

    public Task<PagedResult<PedidoDto>> ListarMeusAsync(Guid clienteId, PedidoFiltroRequest filtro, CancellationToken cancellationToken = default)
        => BuscarAsync(filtro, clienteId, false, cancellationToken);

    public async Task<PedidoDto> ObterAsync(Guid pedidoId, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidos.GetByIdAsync(pedidoId, cancellationToken)
            ?? throw new NotFoundException("Pedido não encontrado.");
        return pedido.ToDto();
    }

    public async Task<PedidoDto> AtualizarAsync(Guid usuarioId, Guid pedidoId, AtualizarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        RequestValidator.ValidateObject(request);
        var valor = ValidarValor(request.ValorInicial);

        var pedido = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            // Bloqueia o pedido: impede edição concorrente com o envio/aceite de propostas.
            var atual = await _pedidos.GetByIdParaAtualizacaoAsync(pedidoId, ct)
                ?? throw new NotFoundException("Pedido não encontrado.");

            // Regra 7: apenas o proprietário edita.
            if (!atual.PertenceA(usuarioId))
                throw new ForbiddenException("Apenas o proprietário do pedido pode editá-lo.");

            // Regras 8 e 9: a entidade rejeita edição de pedidos que não estejam Abertos (DomainException -> 409).
            // Isso inclui pedidos EmNegociacao: o valor inicial não muda depois que há propostas.
            atual.Atualizar(request.Titulo.Trim(), request.Descricao.Trim(), NormalizarCategoria(request.Categoria), valor);
            return atual;
        }, cancellationToken);

        return pedido.ToDto();
    }

    public async Task CancelarAsync(Guid usuarioId, Guid pedidoId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var pedido = await _pedidos.GetByIdParaAtualizacaoAsync(pedidoId, ct)
                ?? throw new NotFoundException("Pedido não encontrado.");

            if (!pedido.PertenceA(usuarioId))
                throw new ForbiddenException("Apenas o proprietário do pedido pode cancelá-lo.");

            pedido.Cancelar();

            // Propostas pendentes de um pedido cancelado deixam de valer; o histórico é preservado.
            var pendentes = await _propostas.ListarPendentesDoPedidoAsync(pedido.Id, null, ct);
            foreach (var proposta in pendentes)
                proposta.MarcarComoNaoSelecionada();

            return true;
        }, cancellationToken);
    }

    private async Task<PagedResult<PedidoDto>> BuscarAsync(PedidoFiltroRequest filtro, Guid? clienteId, bool apenasDisponiveis, CancellationToken cancellationToken)
    {
        RequestValidator.ValidateObject(filtro);

        var erros = new ValidationErrors();
        if (filtro.ValorMinimo < 0)
            erros.Add(nameof(filtro.ValorMinimo), "O valor mínimo não pode ser negativo.");
        if (filtro.ValorMaximo < 0)
            erros.Add(nameof(filtro.ValorMaximo), "O valor máximo não pode ser negativo.");
        if (filtro.ValorMinimo is not null && filtro.ValorMaximo is not null && filtro.ValorMinimo > filtro.ValorMaximo)
            erros.Add(nameof(filtro.ValorMinimo), "O valor mínimo não pode ser maior que o valor máximo.");
        if (filtro.Status is not null && !Enum.IsDefined(typeof(StatusPedido), filtro.Status.Value))
            erros.Add(nameof(filtro.Status), "Status inválido.");
        erros.ThrowIfAny();

        var (itens, total) = await _pedidos.BuscarAsync(new PedidoFiltro
        {
            ClienteId = clienteId,
            ApenasDisponiveis = apenasDisponiveis,
            Titulo = filtro.Titulo,
            Categoria = filtro.Categoria,
            Status = filtro.Status,
            ValorMinimo = filtro.ValorMinimo,
            ValorMaximo = filtro.ValorMaximo,
            Pagina = filtro.Pagina,
            TamanhoPagina = filtro.TamanhoPagina
        }, cancellationToken);

        var totalPaginas = (int)Math.Ceiling(total / (double)filtro.TamanhoPagina);
        return new PagedResult<PedidoDto>(
            itens.Select(p => p.ToDto()).ToList(),
            filtro.Pagina,
            filtro.TamanhoPagina,
            total,
            totalPaginas);
    }

    /// <summary>Regra 4: ValorInicial deve ser maior que zero (considerando 2 casas decimais).</summary>
    private static decimal ValidarValor(decimal valorInicial)
    {
        var valor = decimal.Round(valorInicial, 2, MidpointRounding.AwayFromZero);
        var erros = new ValidationErrors();
        if (valor <= 0)
            erros.Add(nameof(CriarPedidoRequest.ValorInicial), "O valor inicial deve ser maior que zero.");
        else if (valor > ValorMaximo)
            erros.Add(nameof(CriarPedidoRequest.ValorInicial), "O valor inicial excede o máximo permitido.");
        erros.ThrowIfAny();
        return valor;
    }

    private static string NormalizarCategoria(string? categoria)
        => string.IsNullOrWhiteSpace(categoria) ? CategoriaPadrao : categoria.Trim();
}
