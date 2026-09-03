using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Infraestructura.Integraciones.SmartNova.Configuraciones;

public sealed class EmpresaSmartNovaConfiguracion :
    IEntityTypeConfiguration<EmpresaSmartNova>
{
    public void Configure(EntityTypeBuilder<EmpresaSmartNova> builder)
    {
        builder.ToTable("Empresa", tabla => tabla.ExcludeFromMigrations());
        builder.HasKey(empresa => empresa.Id);
        builder.Property(empresa => empresa.NombreLegal).HasMaxLength(256);
        builder.Property(empresa => empresa.Telefono).HasPrecision(18, 0);
        builder.Property(empresa => empresa.Dirreccion).HasColumnName("Dirreccion");
    }
}
