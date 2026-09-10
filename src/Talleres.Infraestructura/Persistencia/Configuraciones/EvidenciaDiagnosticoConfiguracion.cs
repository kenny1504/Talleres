using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class EvidenciaDiagnosticoConfiguracion :
    IEntityTypeConfiguration<EvidenciaDiagnostico>
{
    public void Configure(EntityTypeBuilder<EvidenciaDiagnostico> builder)
    {
        builder.ToTable("EvidenciasDiagnostico");
        builder.HasKey(evidencia => evidencia.Id);
        builder.Property(evidencia => evidencia.ClaveObjeto).HasMaxLength(500).IsRequired();
        builder.Property(evidencia => evidencia.NombreArchivo).HasMaxLength(255).IsRequired();
        builder.Property(evidencia => evidencia.TipoContenido).HasMaxLength(100).IsRequired();
        builder.Property(evidencia => evidencia.FechaCargaUtc).HasPrecision(0);
        builder.HasIndex(evidencia => evidencia.ClaveObjeto).IsUnique();
        builder.HasIndex(evidencia => new { evidencia.EmpresaId, evidencia.OrdenServicioId });
        builder.HasOne(evidencia => evidencia.OrdenServicio)
            .WithMany(orden => orden.EvidenciasDiagnostico)
            .HasForeignKey(evidencia => evidencia.OrdenServicioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
