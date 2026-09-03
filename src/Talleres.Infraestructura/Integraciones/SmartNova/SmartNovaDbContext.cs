using Microsoft.EntityFrameworkCore;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Infraestructura.Integraciones.SmartNova;

public sealed class SmartNovaDbContext(DbContextOptions<SmartNovaDbContext> options) :
    DbContext(options)
{
    public DbSet<UsuarioSmartNova> Usuarios => Set<UsuarioSmartNova>();

    public DbSet<InicioSesionExternoSmartNova> IniciosSesionExternos => Set<InicioSesionExternoSmartNova>();

    public DbSet<EmpresaSmartNova> Empresas => Set<EmpresaSmartNova>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SmartNovaDbContext).Assembly,
            tipo => tipo.Namespace?.Contains(
                ".Integraciones.SmartNova.Configuraciones",
                StringComparison.Ordinal) == true);
        base.OnModelCreating(modelBuilder);
    }
}
