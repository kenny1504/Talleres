using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class PagoClienteConfiguracion : IEntityTypeConfiguration<PagoCliente>
{
    public void Configure(EntityTypeBuilder<PagoCliente> builder)
    {
        builder.ToTable("PagosClientes");
        builder.HasKey(pago => pago.Id);
        builder.Property(pago => pago.Monto).HasPrecision(18, 2);
        builder.Property(pago => pago.FechaRegistroUtc).HasPrecision(0);
        builder.Property(pago => pago.FechaAnulacionUtc).HasPrecision(0);
        builder.Property(pago => pago.FormaPago).HasMaxLength(60);
        builder.Property(pago => pago.Referencia).HasMaxLength(120);

        builder.HasIndex(pago => new { pago.EmpresaId, pago.ClienteId, pago.FechaRegistroUtc });
        builder.HasOne(pago => pago.Cliente)
            .WithMany(cliente => cliente.Pagos)
            .HasForeignKey(pago => new { pago.EmpresaId, pago.ClienteId })
            .HasPrincipalKey(cliente => new { cliente.EmpresaId, cliente.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
