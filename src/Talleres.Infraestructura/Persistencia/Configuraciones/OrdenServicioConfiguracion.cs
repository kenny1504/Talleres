using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class OrdenServicioConfiguracion : IEntityTypeConfiguration<OrdenServicio>
{
    public void Configure(EntityTypeBuilder<OrdenServicio> builder)
    {
        builder.ToTable("OrdenesServicio");
        builder.HasKey(orden => orden.Id);

        builder.Property(orden => orden.Numero)
            .HasMaxLength(25)
            .IsRequired();
        builder.Property(orden => orden.Estado).HasConversion<int>();
        builder.Property(orden => orden.FechaIngreso).HasPrecision(0);
        builder.Property(orden => orden.Observaciones).HasMaxLength(1000);
        builder.Property(orden => orden.Diagnostico).HasMaxLength(4000);
        builder.Property(orden => orden.FechaDiagnosticoUtc).HasPrecision(0);
        builder.Property(orden => orden.TokenPublico).HasMaxLength(64);
        builder.Property(orden => orden.FechaAutorizacionClienteUtc).HasPrecision(0);

        builder.HasIndex(orden => new { orden.EmpresaId, orden.Numero }).IsUnique();
        builder.HasIndex(orden => new { orden.EmpresaId, orden.FechaIngreso });
        builder.HasIndex(orden => orden.TokenPublico)
            .IsUnique()
            .HasFilter("[TokenPublico] IS NOT NULL");

        builder.HasOne(orden => orden.Cliente)
            .WithMany(cliente => cliente.OrdenesServicio)
            .HasForeignKey(orden => orden.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(orden => orden.Vehiculo)
            .WithMany(vehiculo => vehiculo.OrdenesServicio)
            .HasForeignKey(orden => orden.VehiculoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(orden => orden.TecnicoTaller)
            .WithMany(tecnico => tecnico.OrdenesServicio)
            .HasForeignKey(orden => new { orden.EmpresaId, orden.TecnicoTallerId })
            .HasPrincipalKey(tecnico => new { tecnico.EmpresaId, tecnico.Id })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
