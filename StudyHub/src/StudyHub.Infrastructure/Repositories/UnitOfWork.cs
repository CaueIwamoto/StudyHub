using Microsoft.EntityFrameworkCore;
using Npgsql;
using StudyHub.Application.Exceptions;
using StudyHub.Domain.Interfaces;
using StudyHub.Infrastructure.Persistence;
using StudyHub.Infrastructure.Persistence.Configurations;

namespace StudyHub.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly StudyHubDbContext _db;

    public UnitOfWork(StudyHubDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            // Os índices únicos do banco são a garantia final contra corridas entre requisições.
            throw new ConflictException(pg.ConstraintName switch
            {
                PropostaConfiguration.IndicePendentePorPrestador =>
                    "Você já possui uma proposta pendente para este pedido.",
                PropostaConfiguration.IndiceAceitaPorPedido =>
                    "Este pedido já possui uma proposta aceita.",
                _ => "Registro duplicado (e-mail já cadastrado)."
            });
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var resultado = await action(cancellationToken);
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return resultado;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
