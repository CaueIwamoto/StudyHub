using Microsoft.EntityFrameworkCore;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Interfaces;
using StudyHub.Infrastructure.Persistence;

namespace StudyHub.Infrastructure.Repositories;

public sealed class PropostaRepository : IPropostaRepository
{
    private readonly StudyHubDbContext _db;

    public PropostaRepository(StudyHubDbContext db) => _db = db;

    public Task<Proposta?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Propostas
            .Include(p => p.Pedido)
            .Include(p => p.Prestador)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Guid?> ObterPedidoIdAsync(Guid propostaId, CancellationToken cancellationToken = default)
    {
        var pedidoIds = await _db.Propostas
            .AsNoTracking()
            .Where(p => p.Id == propostaId)
            .Select(p => p.PedidoId)
            .ToListAsync(cancellationToken);

        return pedidoIds.Count == 0 ? null : pedidoIds[0];
    }

    public async Task AddAsync(Proposta proposta, CancellationToken cancellationToken = default)
        => await _db.Propostas.AddAsync(proposta, cancellationToken);

    public Task<bool> ExistePendenteDoPrestadorAsync(Guid pedidoId, Guid prestadorId, CancellationToken cancellationToken = default)
        => _db.Propostas.AnyAsync(
            p => p.PedidoId == pedidoId && p.PrestadorId == prestadorId && p.Status == StatusProposta.Pendente,
            cancellationToken);

    public Task<bool> ExisteOutraPendenteAsync(Guid pedidoId, Guid exceto, CancellationToken cancellationToken = default)
        => _db.Propostas.AnyAsync(
            p => p.PedidoId == pedidoId && p.Id != exceto && p.Status == StatusProposta.Pendente,
            cancellationToken);

    public async Task<IReadOnlyList<Proposta>> ListarPendentesDoPedidoAsync(Guid pedidoId, Guid? exceto, CancellationToken cancellationToken = default)
    {
        var query = _db.Propostas.Where(p => p.PedidoId == pedidoId && p.Status == StatusProposta.Pendente);
        if (exceto is { } id)
            query = query.Where(p => p.Id != id);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Proposta>> ListarPorPedidoAsync(Guid pedidoId, StatusProposta? status, CancellationToken cancellationToken = default)
    {
        var query = _db.Propostas
            .AsNoTracking()
            .Include(p => p.Pedido)
            .Include(p => p.Prestador)
            .Where(p => p.PedidoId == pedidoId);
        if (status is { } s)
            query = query.Where(p => p.Status == s);

        return await query
            .OrderByDescending(p => p.DataCriacao)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Proposta>> ListarPorPrestadorAsync(Guid prestadorId, StatusProposta? status, CancellationToken cancellationToken = default)
    {
        var query = _db.Propostas
            .AsNoTracking()
            .Include(p => p.Pedido)
            .Include(p => p.Prestador)
            .Where(p => p.PrestadorId == prestadorId);
        if (status is { } s)
            query = query.Where(p => p.Status == s);

        return await query
            .OrderByDescending(p => p.DataCriacao)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }
}
