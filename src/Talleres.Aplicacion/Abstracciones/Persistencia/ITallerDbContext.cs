using Microsoft.EntityFrameworkCore;
using Talleres.Dominio.Entidades;

namespace Talleres.Aplicacion.Abstracciones.Persistencia;

/// <summary>
/// Expone las unidades de datos requeridas por los servicios de aplicación.
/// </summary>
public interface ITallerDbContext
{
    DbSet<Cliente> Clientes { get; }

    DbSet<Vehiculo> Vehiculos { get; }

    DbSet<OrdenServicio> OrdenesServicio { get; }

    DbSet<RecepcionVehiculo> RecepcionesVehiculo { get; }

    DbSet<DanioVehiculo> DaniosVehiculo { get; }

    DbSet<HistorialOrdenServicio> HistorialOrdenesServicio { get; }

    /// <summary>
    /// Persiste de forma atómica los cambios pendientes en el contexto.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Cantidad de registros afectados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
