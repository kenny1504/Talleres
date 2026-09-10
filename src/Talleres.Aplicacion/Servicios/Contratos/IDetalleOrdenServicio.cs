using Talleres.Aplicacion.DTOs.OrdenesServicio;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Administra los conceptos cobrados y el consumo de inventario de una orden.
/// </summary>
public interface IDetalleOrdenServicio
{
    /// <summary>Obtiene los detalles y el total vigente de una orden de la empresa actual.</summary>
    /// <param name="ordenServicioId">Orden que se consultará.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Detalles persistidos y total calculado.</returns>
    Task<ResumenDetallesOrdenServicioDto> ObtenerAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Agrega un producto con el precio del inventario y descuenta stock cuando la cantidad disponible es suficiente.
    /// </summary>
    /// <param name="ordenServicioId">Orden en reparación.</param>
    /// <param name="solicitud">Producto, bodega y cantidad solicitada.</param>
    /// <param name="empresaNovaId">Empresa autenticada en NOVA.</param>
    /// <param name="usuarioId">Usuario autenticado que origina la salida.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resumen actualizado de la orden.</returns>
    Task<ResumenDetallesOrdenServicioDto> AgregarInventarioAsync(
        long ordenServicioId,
        AgregarDetalleInventarioSolicitud solicitud,
        int empresaNovaId,
        string usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>Agrega un producto comprado, mano de obra u otro concepto capturado manualmente.</summary>
    /// <param name="ordenServicioId">Orden en reparación.</param>
    /// <param name="solicitud">Descripción, cantidad, unidad y precio.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resumen actualizado de la orden.</returns>
    Task<ResumenDetallesOrdenServicioDto> AgregarManualAsync(
        long ordenServicioId,
        AgregarDetalleManualSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>Elimina un concepto que todavía no haya producido un movimiento de inventario.</summary>
    /// <param name="ordenServicioId">Orden propietaria del detalle.</param>
    /// <param name="detalleId">Detalle que se eliminará.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resumen actualizado de la orden.</returns>
    Task<ResumenDetallesOrdenServicioDto> EliminarAsync(
        long ordenServicioId,
        long detalleId,
        CancellationToken cancellationToken = default);
}
