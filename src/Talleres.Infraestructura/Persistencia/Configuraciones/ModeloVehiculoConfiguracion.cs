using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class ModeloVehiculoConfiguracion : IEntityTypeConfiguration<ModeloVehiculo>
{
    public void Configure(EntityTypeBuilder<ModeloVehiculo> builder)
    {
        builder.ToTable("ModelosVehiculo");
        builder.HasKey(modelo => modelo.Id);
        builder.Property(modelo => modelo.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(modelo => modelo.FechaCreacion).HasPrecision(0);
        builder.HasIndex(modelo => new { modelo.EmpresaId, modelo.MarcaVehiculoId, modelo.Nombre })
            .IsUnique();
        builder.HasAlternateKey(modelo => new { modelo.EmpresaId, modelo.Id });
        builder.HasOne(modelo => modelo.Marca)
            .WithMany(marca => marca.Modelos)
            .HasForeignKey(modelo => new { modelo.EmpresaId, modelo.MarcaVehiculoId })
            .HasPrincipalKey(marca => new { marca.EmpresaId, marca.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
