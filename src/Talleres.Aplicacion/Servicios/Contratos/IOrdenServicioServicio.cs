using Talleres.Aplicacion.DTOs.OrdenesServicio;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Coordina el ciclo de vida de las órdenes de servicio de la empresa actual.
/// </summary>
public interface IOrdenServicioServicio
{
    /// <summary>
    /// Crea una orden para un cliente y vehículo relacionados y registra su historial inicial.
    /// </summary>
    /// <param name="solicitud">Cliente, vehículo y observaciones iniciales.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La orden creada en estado de recepción.</returns>
    Task<OrdenServicioDto> CrearAsync(
        CrearOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una orden por su identificador dentro de la empresa actual.
    /// </summary>
    /// <param name="ordenServicioId">Identificador interno de la orden.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>La orden encontrada con sus datos principales.</returns>
    Task<OrdenServicioDto> ObtenerPorIdAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista las órdenes visibles para la empresa actual, de la más reciente a la más antigua.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Colección de órdenes de servicio.</returns>
    Task<IReadOnlyCollection<OrdenServicioDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el estado de una orden cuando la transición está permitida y registra el historial.
    /// </summary>
    /// <param name="ordenServicioId">Identificador de la orden que cambiará de estado.</param>
    /// <param name="solicitud">Nuevo estado y motivo de la transición.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La orden después de aplicar la transición.</returns>
    Task<OrdenServicioDto> CambiarEstadoAsync(
        long ordenServicioId,
        CambiarEstadoOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
