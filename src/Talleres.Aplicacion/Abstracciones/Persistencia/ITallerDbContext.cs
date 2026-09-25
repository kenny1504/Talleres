using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Talleres.Dominio.Entidades;

namespace Talleres.Aplicacion.Abstracciones.Persistencia;

/// <summary>
/// Expone las unidades de datos requeridas por los servicios de aplicación.
/// </summary>
public interface ITallerDbContext
{
    DbSet<Cliente> Clientes { get; }

    DbSet<Vehiculo> Vehiculos { get; }

    DbSet<MarcaVehiculo> MarcasVehiculo { get; }

    DbSet<ModeloVehiculo> ModelosVehiculo { get; }

    DbSet<OrdenServicio> OrdenesServicio { get; }

    DbSet<PagoCliente> PagosClientes { get; }

    DbSet<TecnicoTaller> TecnicosTaller { get; }

    DbSet<RecepcionVehiculo> RecepcionesVehiculo { get; }

    DbSet<DanioVehiculo> DaniosVehiculo { get; }

    DbSet<EvidenciaInspeccion> EvidenciasInspeccion { get; }

    DbSet<EvidenciaDiagnostico> EvidenciasDiagnostico { get; }

    DbSet<HistorialOrdenServicio> HistorialOrdenesServicio { get; }

    DbSet<DetalleOrdenServicio> DetallesOrdenesServicio { get; }

    DbSet<TallerSincronizado> TalleresSincronizados { get; }

    /// <summary>
    /// Persiste de forma atómica los cambios pendientes en el contexto.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Cantidad de registros afectados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Inicia una transacción para cambios relacionados que necesitan varios guardados.</summary>
    Task<IDbContextTransaction> IniciarTransaccionAsync(CancellationToken cancellationToken = default);
}
