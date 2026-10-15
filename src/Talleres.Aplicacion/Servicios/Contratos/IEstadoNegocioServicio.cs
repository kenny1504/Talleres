using Talleres.Aplicacion.DTOs.EstadoNegocio;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>Consulta los cobros, la cartera y la actividad del taller autorizado.</summary>
public interface IEstadoNegocioServicio
{
    /// <summary>
    /// Resume los abonos vigentes y las órdenes no canceladas del período.
    /// La cartera conserva el saldo actual completo de cada cliente, independientemente del período.
    /// </summary>
    /// <param name="solicitud">Fechas locales inclusivas, desfase respecto a UTC y agrupación del gráfico. Máximo 366 días.</param>
    /// <param name="cancellationToken">Permite cancelar las consultas de lectura.</param>
    /// <returns>Indicadores y clasificaciones de la empresa activa, sin asignar pagos a órdenes ni compensar deudas entre clientes.</returns>
    Task<EstadoNegocioDto> ObtenerAsync(
        ConsultarEstadoNegocioSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los pagos de clientes de la empresa activa, del más reciente al más antiguo.
    /// Conserva los anulados e incluye todo el historial cuando no se indican fechas.
    /// </summary>
    /// <param name="solicitud">Fechas locales opcionales (ambas o ninguna), situación, búsqueda y paginación de hasta 50 registros.</param>
    /// <param name="cancellationToken">Permite cancelar el conteo y la consulta de pagos.</param>
    /// <returns>Página de pagos con datos del cliente y total de coincidencias, sin asignar los abonos a órdenes.</returns>
    Task<PagosNegocioDto> ListarPagosAsync(
        ConsultarPagosNegocioSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
