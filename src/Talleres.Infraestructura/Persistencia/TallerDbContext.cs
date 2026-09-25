using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public DbSet<MarcaVehiculo> MarcasVehiculo => Set<MarcaVehiculo>();

    public DbSet<ModeloVehiculo> ModelosVehiculo => Set<ModeloVehiculo>();

    public DbSet<OrdenServicio> OrdenesServicio => Set<OrdenServicio>();

    public DbSet<PagoCliente> PagosClientes => Set<PagoCliente>();

    public DbSet<TecnicoTaller> TecnicosTaller => Set<TecnicoTaller>();

    public DbSet<RecepcionVehiculo> RecepcionesVehiculo => Set<RecepcionVehiculo>();

    public DbSet<DanioVehiculo> DaniosVehiculo => Set<DanioVehiculo>();

    public DbSet<EvidenciaInspeccion> EvidenciasInspeccion => Set<EvidenciaInspeccion>();

    public DbSet<EvidenciaDiagnostico> EvidenciasDiagnostico => Set<EvidenciaDiagnostico>();

    public DbSet<HistorialOrdenServicio> HistorialOrdenesServicio =>
        Set<HistorialOrdenServicio>();

    public DbSet<DetalleOrdenServicio> DetallesOrdenesServicio =>
        Set<DetalleOrdenServicio>();

    public DbSet<TallerSincronizado> TalleresSincronizados =>
        Set<TallerSincronizado>();

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ValidarAislamientoEmpresa();
        return base.SaveChangesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> IniciarTransaccionAsync(CancellationToken cancellationToken = default) =>
        Database.BeginTransactionAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TallerDbContext).Assembly,
            tipo => tipo.Namespace?.Equals(
                "Talleres.Infraestructura.Persistencia.Configuraciones",
                StringComparison.Ordinal) == true);

        modelBuilder.Entity<Cliente>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<Vehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<MarcaVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<ModeloVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<OrdenServicio>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<PagoCliente>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<TecnicoTaller>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<RecepcionVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<DanioVehiculo>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<EvidenciaInspeccion>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<EvidenciaDiagnostico>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<HistorialOrdenServicio>()
            .HasQueryFilter(entidad => entidad.EmpresaId == contextoEmpresa.EmpresaId);
        modelBuilder.Entity<DetalleOrdenServicio>()
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
