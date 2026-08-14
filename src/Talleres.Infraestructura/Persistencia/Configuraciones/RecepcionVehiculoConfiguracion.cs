using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class RecepcionVehiculoConfiguracion : IEntityTypeConfiguration<RecepcionVehiculo>
{
    public void Configure(EntityTypeBuilder<RecepcionVehiculo> builder)
    {
        builder.ToTable(
            "RecepcionesVehiculo",
            tabla => tabla.HasCheckConstraint(
                "CK_RecepcionesVehiculo_PorcentajeCombustible",
                "[PorcentajeCombustible] >= 0 AND [PorcentajeCombustible] <= 100"));
        builder.HasKey(recepcion => recepcion.Id);

        builder.Property(recepcion => recepcion.DescripcionEstado)
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(recepcion => recepcion.FechaRecepcion).HasPrecision(0);

        builder.HasIndex(recepcion => new { recepcion.EmpresaId, recepcion.OrdenServicioId })
            .IsUnique();

        builder.HasOne(recepcion => recepcion.OrdenServicio)
            .WithOne(orden => orden.Recepcion)
            .HasForeignKey<RecepcionVehiculo>(recepcion => recepcion.OrdenServicioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
