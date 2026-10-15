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
}
