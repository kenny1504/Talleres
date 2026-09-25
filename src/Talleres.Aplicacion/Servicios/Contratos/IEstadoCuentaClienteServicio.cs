using Talleres.Aplicacion.DTOs.Clientes;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Consulta los cargos y abonos de un cliente de la empresa activa.
/// </summary>
public interface IEstadoCuentaClienteServicio
{
    /// <summary>
    /// Calcula el saldo actual con las órdenes no canceladas y los pagos vigentes.
    /// </summary>
    /// <param name="clienteId">Cliente cuya cuenta se consulta.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Órdenes, pagos y saldo global del cliente.</returns>
    Task<EstadoCuentaClienteDto> ObtenerAsync(long clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un abono al cliente sin asignarlo a una orden específica.
    /// </summary>
    /// <param name="clienteId">Cliente que efectuó el pago.</param>
    /// <param name="solicitud">Monto requerido y datos opcionales del pago.</param>
    /// <param name="cancellationToken">Token para cancelar el registro.</param>
    /// <returns>Estado de cuenta recalculado después del abono.</returns>
    Task<EstadoCuentaClienteDto> RegistrarPagoAsync(
        long clienteId,
        RegistrarPagoClienteSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Anula un pago conservando el registro y restituye su efecto en el saldo.
    /// </summary>
    /// <param name="clienteId">Cliente propietario del pago.</param>
    /// <param name="pagoId">Pago que se anula.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Estado de cuenta recalculado después de la anulación.</returns>
    Task<EstadoCuentaClienteDto> AnularPagoAsync(
        long clienteId,
        long pagoId,
        CancellationToken cancellationToken = default);
}
