using Microsoft.EntityFrameworkCore;
using StudyHub.Domain.Entities;
using StudyHub.Domain.Enums;
using StudyHub.Domain.Interfaces;
using StudyHub.Infrastructure.Persistence;

namespace StudyHub.Infrastructure.Repositories;

public sealed class PedidoRepository : IPedidoRepository
{
    private readonly StudyHubDbContext _db;

    public PedidoRepository(StudyHubDbContext db) => _db = db;

    public Task<Pedido?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _db.Pedidos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Pedido?> GetByIdParaAtualizacaoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_db.Database.IsNpgsql())
            return await GetByIdAsync(id, cancellationToken); // providers sem lock de linha (ex.: InMemory nos testes)

        // Bloqueio pessimista da linha: transações concorrentes sobre o mesmo pedido são serializadas.
        // ToListAsync (e não FirstOrDefaultAsync) evita que o EF envolva o SQL em uma subconsulta.
        var pedidos = await _db.Pedidos
            .FromSqlInterpolated($"SELECT * FROM \"Pedidos\" WHERE \"Id\" = {id} FOR UPDATE")
            .ToListAsync(cancellationToken);
        return pedidos.FirstOrDefault();
    }

    public async Task AddAsync(Pedido pedido, CancellationToken cancellationToken = default)
        => await _db.Pedidos.AddAsync(pedido, cancellationToken);

    public async Task<(IReadOnlyList<Pedido> Itens, int Total)> BuscarAsync(PedidoFiltro filtro, CancellationToken cancellationToken = default)
    {
        IQueryable<Pedido> query = _db.Pedidos.AsNoTracking();

        if (filtro.ClienteId is { } clienteId)
            query = query.Where(p => p.ClienteId == clienteId);

        if (!string.IsNullOrWhiteSpace(filtro.Titulo))
        {
            var titulo = filtro.Titulo.Trim().ToLower();
            query = query.Where(p => p.Titulo.ToLower().Contains(titulo));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            var categoria = filtro.Categoria.Trim().ToLower();
            query = query.Where(p => p.Categoria.ToLower() == categoria);
        }

        if (filtro.ApenasDisponiveis)
            query = query.Where(p => p.Status == StatusPedido.Aberto || p.Status == StatusPedido.EmNegociacao);

        if (filtro.Status is { } status)
            query = query.Where(p => p.Status == status);

        if (filtro.ValorMinimo is { } minimo)
            query = query.Where(p => p.ValorInicial >= minimo);

        if (filtro.ValorMaximo is { } maximo)
            query = query.Where(p => p.ValorInicial <= maximo);

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(p => p.DataCriacao)
            .ThenBy(p => p.Id)
            .Skip((filtro.Pagina - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, total);
    }
}
