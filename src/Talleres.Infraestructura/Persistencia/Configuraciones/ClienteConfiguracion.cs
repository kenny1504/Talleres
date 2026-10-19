using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Nombre)
            .HasMaxLength(150)
            .IsRequired();
        builder.Property(cliente => cliente.Telefono)
            .HasMaxLength(30)
            .IsRequired();
        // Debe quitar los mismos separadores que Cliente.NormalizarTelefono.
        builder.Property(cliente => cliente.TelefonoNormalizado)
            .HasMaxLength(30)
            .IsRequired()
            .HasComputedColumnSql(
                "REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE([Telefono], N' ', N''), N'-', N''), N'(', N''), N')', N''), N'+', N''), N'.', N''), NCHAR(9), N''), NCHAR(13), N''), NCHAR(10), N''), NCHAR(160), N'')",
                stored: true);
        builder.Property(cliente => cliente.Direccion).HasMaxLength(300);
        builder.Property(cliente => cliente.FechaCreacion).HasPrecision(0);

        builder.HasIndex(cliente => new { cliente.EmpresaId, cliente.Nombre });
        builder.HasIndex(cliente => new { cliente.EmpresaId, cliente.TelefonoNormalizado })
            .IsUnique();
    }
}
