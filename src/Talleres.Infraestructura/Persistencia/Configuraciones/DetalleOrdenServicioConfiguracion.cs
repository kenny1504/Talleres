using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class DetalleOrdenServicioConfiguracion :
    IEntityTypeConfiguration<DetalleOrdenServicio>
{
    public void Configure(EntityTypeBuilder<DetalleOrdenServicio> builder)
    {
        builder.ToTable("DetallesOrdenesServicio");
        builder.HasKey(detalle => detalle.Id);
        builder.Property(detalle => detalle.Tipo).HasConversion<int>();
        builder.Property(detalle => detalle.CodigoProducto).HasMaxLength(80);
        builder.Property(detalle => detalle.Descripcion).HasMaxLength(300).IsRequired();
        builder.Property(detalle => detalle.UnidadMedida).HasMaxLength(50);
        builder.Property(detalle => detalle.Cantidad).HasPrecision(18, 4);
        builder.Property(detalle => detalle.PrecioUnitario).HasPrecision(18, 4);
        builder.Property(detalle => detalle.FechaCreacion).HasPrecision(0);

        builder.HasIndex(detalle => new { detalle.EmpresaId, detalle.OrdenServicioId });
        builder.HasIndex(detalle => new { detalle.EmpresaId, detalle.SalidaInventarioId });

        builder.HasOne(detalle => detalle.OrdenServicio)
            .WithMany(orden => orden.Detalles)
            .HasForeignKey(detalle => detalle.OrdenServicioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
