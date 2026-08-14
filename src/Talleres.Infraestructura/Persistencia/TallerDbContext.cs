using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Infraestructura.Persistencia;

public sealed class TallerDbContext(
    DbContextOptions<TallerDbContext> options,
    IContextoEmpresa contextoEmpresa) : DbContext(options), ITallerDbContext
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();

    public DbSet<OrdenServicio> OrdenesServicio => Set<OrdenServicio>();

    public DbSet<RecepcionVehiculo> RecepcionesVehiculo => Set<RecepcionVehiculo>();

    public DbSet<DanioVehiculo> DaniosVehiculo => Set<DanioVehiculo>();

    public DbSet<HistorialOrdenServicio> HistorialOrdenesServicio =>
        Set<HistorialOrdenServicio>();

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ValidarAislamientoEmpresa();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TallerDbContext).Assembly);

        modelBuilder.Entity<Cliente>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<Vehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<OrdenServicio>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<RecepcionVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<DanioVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<HistorialOrdenServicio>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);

        base.OnModelCreating(modelBuilder);
    }

    private void ValidarAislamientoEmpresa()
    {
        if (!contextoEmpresa.EstaDisponible || contextoEmpresa.EmpresaId <= 0)
        {
            throw new ReglaNegocioException(
                "No se puede guardar información sin una empresa válida.");
        }

        foreach (var entrada in ChangeTracker.Entries<IEntidadEmpresa>())
        {
            if (entrada.State == EntityState.Added)
            {
                if (entrada.Entity.EmpresaId == 0)
                {
                    entrada.Entity.EmpresaId = contextoEmpresa.EmpresaId;
                }

                ValidarEmpresa(entrada.Entity.EmpresaId);
                continue;
            }

            if (entrada.State is EntityState.Modified or EntityState.Deleted)
            {
                ValidarEmpresa(entrada.Property(entidad => entidad.EmpresaId).OriginalValue);
                ValidarEmpresa(entrada.Entity.EmpresaId);
                entrada.Property(entidad => entidad.EmpresaId).IsModified = false;
            }
        }
    }

    private void ValidarEmpresa(long empresaId)
    {
        if (empresaId != contextoEmpresa.EmpresaId)
        {
            throw new ReglaNegocioException(
                "La operación intentó modificar información perteneciente a otra empresa.");
        }
    }
}
