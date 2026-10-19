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

    /// <summary>Inicia una transacción para varios guardados o para proteger lecturas y cambios relacionados.</summary>
    /// <param name="cancellationToken">Token para cancelar la apertura.</param>
    /// <param name="nivelAislamiento">Aislamiento requerido; Serializable impide modificar los registros leídos hasta finalizar.</param>
    /// <returns>Transacción que debe confirmarse y liberarse al finalizar.</returns>
    Task<IDbContextTransaction> IniciarTransaccionAsync(
        CancellationToken cancellationToken = default,
        System.Data.IsolationLevel nivelAislamiento = System.Data.IsolationLevel.ReadCommitted);

    /// <summary>Ejecuta una operación transaccional completa con la política de reintentos del proveedor.</summary>
    /// <typeparam name="TResultado">Tipo del resultado de la operación.</typeparam>
    /// <param name="operacion">Operación que abre y confirma su transacción; sus efectos externos deben admitir reintentos.</param>
    /// <param name="cancellationToken">Token para cancelar la ejecución y las esperas de reintento.</param>
    /// <returns>El resultado confirmado. Un reintento descarta el seguimiento local para volver a leer los datos vigentes.</returns>
    Task<TResultado> EjecutarConEstrategiaAsync<TResultado>(
        Func<Task<TResultado>> operacion,
        CancellationToken cancellationToken = default);
}
