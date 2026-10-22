using Talleres.Aplicacion.DTOs.OrdenesServicio;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>Permite consultar el expediente del vehículo autorizado por un enlace público.</summary>
public interface IExpedienteVehiculoPublicoServicio
{
    /// <summary>Consulta las órdenes del mismo vehículo y empresa que la orden compartida, de más reciente a más antigua.</summary>
    /// <param name="token">Token del diagnóstico compartido; determina el vehículo y la empresa sin aceptar otros identificadores.</param>
    /// <param name="solicitud">Página solicitada, con diez órdenes por página. Una página superior al total devuelve la última.</param>
    /// <param name="cancellationToken">Permite cancelar las consultas y las integraciones de lectura.</param>
    /// <returns>Expediente de consulta, sin tokens de otras órdenes ni acciones de autorización. Un enlace inválido produce recurso no encontrado.</returns>
    Task<ExpedienteVehiculoPublicoDto> ObtenerAsync(string token, ConsultarExpedienteVehiculoPublicoSolicitud solicitud, CancellationToken cancellationToken = default);
}
