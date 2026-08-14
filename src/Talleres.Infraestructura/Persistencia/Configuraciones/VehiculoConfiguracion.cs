using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class VehiculoConfiguracion : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable(
            "Vehiculos",
            tabla => tabla.HasCheckConstraint(
                "CK_Vehiculos_Anio",
                "[Anio] >= 1900 AND [Anio] <= 2100"));
        builder.HasKey(vehiculo => vehiculo.Id);

        builder.Property(vehiculo => vehiculo.Placa)
            .HasMaxLength(15)
            .IsRequired();
        builder.Property(vehiculo => vehiculo.Marca)
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(vehiculo => vehiculo.Modelo)
            .HasMaxLength(80)
            .IsRequired();
        builder.Property(vehiculo => vehiculo.Color).HasMaxLength(40);
        builder.Property(vehiculo => vehiculo.NumeroVin).HasMaxLength(50);
        builder.Property(vehiculo => vehiculo.FechaCreacion).HasPrecision(0);

        builder.HasIndex(vehiculo => new { vehiculo.EmpresaId, vehiculo.Placa })
            .IsUnique();
        builder.HasIndex(vehiculo => new { vehiculo.EmpresaId, vehiculo.ClienteId });

        builder.HasOne(vehiculo => vehiculo.Cliente)
            .WithMany(cliente => cliente.Vehiculos)
            .HasForeignKey(vehiculo => vehiculo.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
