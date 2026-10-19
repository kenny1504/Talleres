namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>Coordina la eliminación de órdenes y la devolución de sus consumos al inventario.</summary>
public interface IEliminacionOrdenServicio
{
    /// <summary>
    /// Elimina una orden que nunca ha llegado a ListaParaEntrega, Entregada o Cerrada.
    /// Devuelve primero las existencias descontadas y elimina recepción, cargos, historial y evidencias.
    /// Si una devolución falla, conserva la orden y registra las devoluciones completadas para poder reintentar.
    /// </summary>
    /// <param name="ordenServicioId">Orden perteneciente a la empresa activa.</param>
    /// <param name="empresaNovaId">Empresa del inventario obtenida de la sesión autenticada.</param>
    /// <param name="usuarioId">Usuario autenticado responsable de las devoluciones.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que finaliza tras eliminar la orden y solicitar la limpieza de sus fotografías.</returns>
    /// <exception cref="Talleres.Dominio.Excepciones.ReglaNegocioException">La orden alcanzó la etapa de entrega o sus consumos no permiten una devolución segura.</exception>
    Task EliminarAsync(
        long ordenServicioId,
        int empresaNovaId,
        string usuarioId,
        CancellationToken cancellationToken = default);
}
