using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Dominio.Entidades;

namespace Talleres.Infraestructura.Persistencia.Configuraciones;

public sealed class ResultadoRevisionOperativaConfiguracion : IEntityTypeConfiguration<ResultadoRevisionOperativa>
{
    public void Configure(EntityTypeBuilder<ResultadoRevisionOperativa> builder)
    {
        builder.ToTable("ResultadosRevisionOperativa");
        builder.HasKey(resultado => resultado.Id);
        builder.Property(resultado => resultado.Actividad).HasConversion<int>();
        builder.HasIndex(resultado => new
        {
            resultado.EmpresaId,
            resultado.RecepcionVehiculoId,
            resultado.Actividad
        }).IsUnique();
        builder.HasOne(resultado => resultado.RecepcionVehiculo)
            .WithMany(recepcion => recepcion.RevisionOperativa)
            .HasForeignKey(resultado => resultado.RecepcionVehiculoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
