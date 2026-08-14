using Talleres.Aplicacion.DTOs.Recepciones;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona la recepción física del vehículo vinculado a una orden.
/// </summary>
public interface IRecepcionVehiculoServicio
{
    /// <summary>
    /// Registra la recepción de una orden en estado de recepción y la avanza a diagnóstico.
    /// </summary>
    /// <param name="ordenServicioId">Identificador de la orden que recibe el vehículo.</param>
    /// <param name="solicitud">Estado físico, kilometraje, combustible y objetos entregados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La recepción creada.</returns>
    Task<RecepcionVehiculoDto> RegistrarAsync(
        long ordenServicioId,
        RegistrarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos y hallazgos de una recepción mientras la orden siga activa en el taller.
    /// </summary>
    /// <param name="ordenServicioId">Identificador de la orden cuya inspección será corregida.</param>
    /// <param name="solicitud">Kilometraje, combustible, observación general y daños actualizados.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La recepción con la inspección visual actualizada.</returns>
    Task<RecepcionVehiculoDto> ActualizarInspeccionAsync(
        long ordenServicioId,
        ActualizarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la recepción asociada a una orden de la empresa actual.
    /// </summary>
    /// <param name="ordenServicioId">Identificador de la orden.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>La recepción registrada para la orden.</returns>
    Task<RecepcionVehiculoDto> ObtenerPorOrdenAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default);
}
