using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class TecnicoTallerConfiguracion : IEntityTypeConfiguration<TecnicoTaller>
{
    public void Configure(EntityTypeBuilder<TecnicoTaller> builder)
    {
        builder.ToTable("TecnicosTaller");
        builder.HasKey(tecnico => tecnico.Id);
        builder.Property(tecnico => tecnico.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(tecnico => tecnico.FechaCreacion).HasPrecision(0);
        builder.HasAlternateKey(tecnico => new { tecnico.EmpresaId, tecnico.Id });
        builder.HasIndex(tecnico => new { tecnico.EmpresaId, tecnico.Nombre }).IsUnique();
        builder.HasIndex(tecnico => new { tecnico.EmpresaId, tecnico.EsPredeterminado })
            .IsUnique()
            .HasFilter("[EsPredeterminado] = 1");
    }
}
