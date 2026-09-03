using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Infraestructura.Integraciones.SmartNova.Configuraciones;

public sealed class UsuarioSmartNovaConfiguracion :
    IEntityTypeConfiguration<UsuarioSmartNova>
{
    public void Configure(EntityTypeBuilder<UsuarioSmartNova> builder)
    {
        builder.ToTable("AspNetUsers", tabla => tabla.ExcludeFromMigrations());
        builder.HasKey(usuario => usuario.Id);
        builder.Property(usuario => usuario.Id).HasMaxLength(450);
        builder.Property(usuario => usuario.UserName).HasMaxLength(256);
        builder.Property(usuario => usuario.NormalizedUserName).HasMaxLength(256);
    }
}
