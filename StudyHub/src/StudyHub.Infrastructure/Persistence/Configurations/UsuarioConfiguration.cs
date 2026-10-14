using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudyHub.Domain.Entities;

namespace StudyHub.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios", t =>
        {
            t.HasCheckConstraint("CK_Usuarios_Tipo", "\"Tipo\" IN (1, 2)");
        });

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Nome).IsRequired().HasMaxLength(120);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(254);
        builder.Property(u => u.SenhaHash).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Tipo).IsRequired();
        builder.Property(u => u.DescricaoPerfil).HasMaxLength(1000);
        builder.Property(u => u.AreasConhecimento).IsRequired().HasColumnType("text[]");
        builder.Property(u => u.DataCadastro).IsRequired().HasColumnType("timestamp with time zone");

        builder.HasIndex(u => u.Email).IsUnique();

        // Usuario 1:N Proposta (prestador) é configurado em PropostaConfiguration.
    }
}
