using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Infraestructura.Integraciones.SmartNova.Configuraciones;

public sealed class InicioSesionExternoSmartNovaConfiguracion : IEntityTypeConfiguration<InicioSesionExternoSmartNova>
{
    public void Configure(EntityTypeBuilder<InicioSesionExternoSmartNova> builder)
    {
        builder.ToTable("AspNetUserLogins", tabla => tabla.ExcludeFromMigrations());
        builder.HasKey(item => new { item.LoginProvider, item.ProviderKey });
        builder.Property(item => item.LoginProvider).HasColumnName("LoginProvider");
        builder.Property(item => item.ProviderKey).HasColumnName("ProviderKey");
        builder.Property(item => item.UserId).HasColumnName("UserId");
    }
}
