using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class MarcaVehiculoConfiguracion : IEntityTypeConfiguration<MarcaVehiculo>
{
    public void Configure(EntityTypeBuilder<MarcaVehiculo> builder)
    {
        builder.ToTable("MarcasVehiculo");
        builder.HasKey(marca => marca.Id);
        builder.Property(marca => marca.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(marca => marca.FechaCreacion).HasPrecision(0);
        builder.HasIndex(marca => new { marca.EmpresaId, marca.Nombre }).IsUnique();
        builder.HasAlternateKey(marca => new { marca.EmpresaId, marca.Id });
    }
}
