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
        builder.Property(cliente => cliente.DocumentoIdentidad)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(cliente => cliente.Telefono)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(cliente => cliente.Correo).HasMaxLength(150);
        builder.Property(cliente => cliente.Direccion).HasMaxLength(300);
        builder.Property(cliente => cliente.FechaCreacion).HasPrecision(0);

        builder.HasIndex(cliente => new { cliente.EmpresaId, cliente.DocumentoIdentidad })
            .IsUnique();
        builder.HasIndex(cliente => new { cliente.EmpresaId, cliente.Nombre });
    }
}
