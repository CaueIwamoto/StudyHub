using Microsoft.EntityFrameworkCore;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Persistence;

public class StudyHubDbContext : DbContext
{
    public StudyHubDbContext(DbContextOptions<StudyHubDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    public DbSet<Proposta> Propostas => Set<Proposta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudyHubDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
