using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class DanioVehiculoConfiguracion : IEntityTypeConfiguration<DanioVehiculo>
{
    public void Configure(EntityTypeBuilder<DanioVehiculo> builder)
    {
        builder.ToTable("DaniosVehiculo");
        builder.HasKey(danio => danio.Id);

        builder.Property(danio => danio.Zona).HasConversion<int>();
        builder.Property(danio => danio.Tipo).HasConversion<int>();
        builder.Property(danio => danio.Severidad).HasConversion<int>();
        builder.Property(danio => danio.Observacion).HasMaxLength(500);

        builder.HasIndex(danio => new { danio.EmpresaId, danio.RecepcionVehiculoId });

        builder.HasOne(danio => danio.RecepcionVehiculo)
            .WithMany(recepcion => recepcion.Danios)
            .HasForeignKey(danio => danio.RecepcionVehiculoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
