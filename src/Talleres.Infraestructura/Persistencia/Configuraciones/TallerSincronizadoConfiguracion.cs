using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class TallerSincronizadoConfiguracion :
    IEntityTypeConfiguration<TallerSincronizado>
{
    public void Configure(EntityTypeBuilder<TallerSincronizado> builder)
    {
        builder.ToTable("TalleresSincronizados");
        builder.HasKey(taller => taller.EmpresaNovaId);
        builder.Property(taller => taller.EmpresaNovaId).ValueGeneratedNever();
        builder.Property(taller => taller.FechaConfiguracionUtc).HasPrecision(0);
        builder.HasIndex(taller => taller.Activo);

        builder.HasData(new TallerSincronizado
        {
            EmpresaNovaId = 3071,
            Activo = true,
            FechaConfiguracionUtc = new DateTime(2026, 8, 27, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
