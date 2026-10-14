using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Persistence.Configurations;

public class PropostaConfiguration : IEntityTypeConfiguration<Proposta>
{
    // Nomes dos índices únicos parciais (também usados pelo UnitOfWork para traduzir violações em 409).
    public const string IndicePendentePorPrestador = "IX_Propostas_PedidoId_PrestadorId_Pendente";
    public const string IndiceAceitaPorPedido = "IX_Propostas_PedidoId_Aceita";

    public void Configure(EntityTypeBuilder<Proposta> builder)
    {
        builder.ToTable("Propostas", t =>
        {
            t.HasCheckConstraint("CK_Propostas_ValorProposto", "\"ValorProposto\" > 0");
            t.HasCheckConstraint("CK_Propostas_Tipo", "\"Tipo\" IN (1, 2)");
            t.HasCheckConstraint("CK_Propostas_Status", "\"Status\" IN (1, 2, 3, 4)");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.ValorProposto).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.Mensagem).HasMaxLength(1000);
        builder.Property(p => p.Tipo).IsRequired();
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.DataCriacao).IsRequired().HasColumnType("timestamp with time zone");

        // Pedido 1:N Propostas (histórico preservado: sem cascade).
        builder.HasOne(p => p.Pedido)
            .WithMany(x => x.Propostas)
            .HasForeignKey(p => p.PedidoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Usuario (Prestador) 1:N Propostas.
        builder.HasOne(p => p.Prestador)
            .WithMany(u => u.Propostas)
            .HasForeignKey(p => p.PrestadorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(p => p.PedidoId);
        builder.HasIndex(p => p.PrestadorId);

        // Regra 5: no máximo uma proposta Pendente (Status = 1) por prestador e pedido.
        builder.HasIndex(p => new { p.PedidoId, p.PrestadorId })
            .IsUnique()
            .HasDatabaseName(IndicePendentePorPrestador)
            .HasFilter("\"Status\" = 1");

        // Regra 19: no máximo uma proposta Aceita (Status = 2) por pedido (garantia final no banco).
        // Índice NOMEADO: sem o nome, o EF trataria como o mesmo índice simples de PedidoId declarado acima.
        builder.HasIndex(p => p.PedidoId, IndiceAceitaPorPedido)
            .IsUnique()
            .HasFilter("\"Status\" = 2");
    }
}
