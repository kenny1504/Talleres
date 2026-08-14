using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class HistorialOrdenServicioConfiguracion :
    IEntityTypeConfiguration<HistorialOrdenServicio>
{
    public void Configure(EntityTypeBuilder<HistorialOrdenServicio> builder)
    {
        builder.ToTable("HistorialOrdenesServicio");
        builder.HasKey(historial => historial.Id);

        builder.Property(historial => historial.EstadoAnterior).HasConversion<int?>();
        builder.Property(historial => historial.EstadoNuevo).HasConversion<int>();
        builder.Property(historial => historial.Descripcion)
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(historial => historial.Fecha).HasPrecision(0);

        builder.HasIndex(historial => new
        {
            historial.EmpresaId,
            historial.OrdenServicioId,
            historial.Fecha
        });

        builder.HasOne(historial => historial.OrdenServicio)
            .WithMany(orden => orden.Historial)
            .HasForeignKey(historial => historial.OrdenServicioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
