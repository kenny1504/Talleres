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
    /// <returns>La orden con sus datos principales y el kilometraje de su recepción, o null si aún no se ha registrado.</returns>
    Task<OrdenServicioDto> ObtenerPorIdAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista las órdenes visibles para la empresa actual, de la más reciente a la más antigua.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Órdenes con el kilometraje registrado en la recepción de cada una, o null si aún no tienen recepción.</returns>
    Task<IReadOnlyCollection<OrdenServicioDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cambia el estado de una orden cuando la transición está permitida y registra el historial.
    /// Iniciar la reparación requiere un diagnóstico guardado; la autorización digital del cliente es opcional.
    /// El historial distingue si existía autorización digital, sin registrarla en nombre del cliente.
    /// </summary>
    /// <param name="ordenServicioId">Identificador de la orden que cambiará de estado.</param>
    /// <param name="solicitud">Nuevo estado y motivo de la transición.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La orden después de aplicar la transición.</returns>
    Task<OrdenServicioDto> CambiarEstadoAsync(
        long ordenServicioId,
        CambiarEstadoOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>Asigna un técnico activo de la empresa a una orden existente.</summary>
    /// <param name="ordenServicioId">Identificador de la orden.</param>
    /// <param name="solicitud">Técnico responsable solicitado.</param>
    /// <param name="cancellationToken">Token para cancelar el cambio.</param>
    /// <returns>La orden con el nuevo responsable.</returns>
    Task<OrdenServicioDto> AsignarTecnicoAsync(
        long ordenServicioId,
        AsignarTecnicoOrdenSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
