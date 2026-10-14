using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Persistence.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos", t =>
        {
            t.HasCheckConstraint("CK_Pedidos_ValorInicial", "\"ValorInicial\" > 0");
            t.HasCheckConstraint("CK_Pedidos_ValorAcordado", "\"ValorAcordado\" IS NULL OR \"ValorAcordado\" > 0");
            t.HasCheckConstraint("CK_Pedidos_Status", "\"Status\" IN (1, 2, 3, 4)");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Titulo).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Descricao).IsRequired().HasMaxLength(4000);
        builder.Property(p => p.Categoria).IsRequired().HasMaxLength(80);
        builder.Property(p => p.ValorInicial).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.ValorAcordado).HasPrecision(18, 2);
        builder.Property(p => p.Status).IsRequired();
        builder.Property(p => p.DataCriacao).IsRequired().HasColumnType("timestamp with time zone");

        builder.HasOne(p => p.Cliente)
            .WithMany(u => u.Pedidos)
            .HasForeignKey(p => p.ClienteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(p => p.ClienteId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Categoria);
        builder.HasIndex(p => p.DataCriacao);

        // Pedido 1:N Proposta é configurado em PropostaConfiguration.
    }
}
