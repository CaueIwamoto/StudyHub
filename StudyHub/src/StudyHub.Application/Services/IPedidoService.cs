using StudyHub.Application.DTOs;

namespace StudyHub.Application.Services;

public interface IPedidoService
{
    Task<PedidoDto> CriarAsync(Guid usuarioId, CriarPedidoRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<PedidoDto>> ListarAsync(PedidoFiltroRequest filtro, CancellationToken cancellationToken = default);
    /// <summary>Pedidos que ainda recebem propostas (Aberto ou EmNegociacao), para o Prestador.</summary>
    Task<PagedResult<PedidoDto>> ListarDisponiveisAsync(PedidoFiltroRequest filtro, CancellationToken cancellationToken = default);
    Task<PagedResult<PedidoDto>> ListarMeusAsync(Guid clienteId, PedidoFiltroRequest filtro, CancellationToken cancellationToken = default);
    Task<PedidoDto> ObterAsync(Guid pedidoId, CancellationToken cancellationToken = default);
    Task<PedidoDto> AtualizarAsync(Guid usuarioId, Guid pedidoId, AtualizarPedidoRequest request, CancellationToken cancellationToken = default);
    Task CancelarAsync(Guid usuarioId, Guid pedidoId, CancellationToken cancellationToken = default);
}
