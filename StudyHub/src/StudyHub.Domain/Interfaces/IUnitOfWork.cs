namespace StudyHub.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa a ação dentro de uma transação de banco de dados (commit ao final, rollback em caso de erro).
    /// Usado pelo Gerenciamento de Propostas (aceite/recusa + mudança de status do pedido de forma atômica).
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default);
}
