using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class OrdenServicioConfiguracion : IEntityTypeConfiguration<OrdenServicio>
{
    public void Configure(EntityTypeBuilder<OrdenServicio> builder)
    {
        builder.ToTable("OrdenesServicio");
        builder.HasKey(orden => orden.Id);

        builder.Property(orden => orden.Numero)
            .HasMaxLength(25)
            .IsRequired();
        builder.Property(orden => orden.Estado).HasConversion<int>();
        builder.Property(orden => orden.FechaIngreso).HasPrecision(0);
        builder.Property(orden => orden.Observaciones).HasMaxLength(1000);

        builder.HasIndex(orden => new { orden.EmpresaId, orden.Numero }).IsUnique();
        builder.HasIndex(orden => new { orden.EmpresaId, orden.FechaIngreso });

        builder.HasOne(orden => orden.Cliente)
            .WithMany(cliente => cliente.OrdenesServicio)
            .HasForeignKey(orden => orden.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(orden => orden.Vehiculo)
            .WithMany(vehiculo => vehiculo.OrdenesServicio)
            .HasForeignKey(orden => orden.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
