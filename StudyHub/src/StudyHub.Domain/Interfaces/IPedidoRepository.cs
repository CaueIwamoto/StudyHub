using StudyHub.Domain.Entities;

namespace StudyHub.Domain.Interfaces;

public interface IPedidoRepository
{
    /// <summary>Retorna a entidade rastreada (tracking) para permitir alterações.</summary>
    Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retorna o pedido rastreado e BLOQUEIA a linha até o fim da transação corrente (SELECT ... FOR UPDATE no PostgreSQL).
    /// Deve ser chamado dentro de <see cref="IUnitOfWork.ExecuteInTransactionAsync{T}"/>, antes de carregar outras
    /// entidades relacionadas, para serializar operações concorrentes sobre o mesmo pedido.
    /// </summary>
    Task<Pedido?> GetByIdParaAtualizacaoAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Pedido> Itens, int Total)> BuscarAsync(PedidoFiltro filtro, CancellationToken cancellationToken = default);
}
