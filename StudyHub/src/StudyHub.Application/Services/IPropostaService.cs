using StudyHub.Application.DTOs;
using StudyHub.Domain.Enums;

namespace StudyHub.Application.Services;

public interface IPropostaService
{
    /// <summary>Prestador envia proposta (aceite do valor inicial ou contraproposta) para um pedido.</summary>
    Task<PropostaResponse> CriarAsync(Guid prestadorId, Guid pedidoId, CriarPropostaRequest request, CancellationToken cancellationToken = default);

    /// <summary>Cliente proprietário lista as propostas recebidas no pedido.</summary>
    Task<IReadOnlyList<PropostaDetalhadaResponse>> ListarPorPedidoAsync(Guid clienteId, Guid pedidoId, StatusProposta? status, CancellationToken cancellationToken = default);

    /// <summary>Prestador lista suas próprias propostas.</summary>
    Task<IReadOnlyList<PropostaDetalhadaResponse>> ListarMinhasAsync(Guid prestadorId, StatusProposta? status, CancellationToken cancellationToken = default);

    /// <summary>Consulta uma proposta (somente o prestador autor ou o cliente proprietário do pedido).</summary>
    Task<PropostaDetalhadaResponse> ObterAsync(Guid usuarioId, Guid propostaId, CancellationToken cancellationToken = default);

    /// <summary>Cliente proprietário aceita a proposta: fecha o pedido, registra o ValorAcordado e encerra as demais.</summary>
    Task<PropostaDetalhadaResponse> AceitarAsync(Guid clienteId, Guid propostaId, CancellationToken cancellationToken = default);

    /// <summary>Cliente proprietário recusa a proposta; se não restarem pendentes, o pedido volta para Aberto.</summary>
    Task<PropostaDetalhadaResponse> RecusarAsync(Guid clienteId, Guid propostaId, CancellationToken cancellationToken = default);
}
