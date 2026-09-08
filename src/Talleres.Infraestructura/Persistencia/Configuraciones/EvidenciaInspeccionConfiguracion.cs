using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class EvidenciaInspeccionConfiguracion : IEntityTypeConfiguration<EvidenciaInspeccion>
{
    public void Configure(EntityTypeBuilder<EvidenciaInspeccion> builder)
    {
        builder.ToTable("EvidenciasInspeccion");
        builder.HasKey(evidencia => evidencia.Id);

        builder.Property(evidencia => evidencia.ClaveObjeto)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(evidencia => evidencia.NombreArchivo)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(evidencia => evidencia.TipoContenido)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(evidencia => evidencia.FechaCargaUtc).HasPrecision(0);

        builder.HasIndex(evidencia => evidencia.ClaveObjeto).IsUnique();
        builder.HasIndex(evidencia => new
        {
            evidencia.EmpresaId,
            evidencia.RecepcionVehiculoId
        });

        builder.HasOne(evidencia => evidencia.RecepcionVehiculo)
            .WithMany(recepcion => recepcion.Evidencias)
            .HasForeignKey(evidencia => evidencia.RecepcionVehiculoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
