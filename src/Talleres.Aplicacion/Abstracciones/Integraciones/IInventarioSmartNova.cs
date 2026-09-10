using Talleres.Aplicacion.DTOs.Inventario;

namespace Talleres.Aplicacion.Abstracciones.Integraciones;

public interface IInventarioSmartNova
{
    /// <summary>Lista las bodegas disponibles de una empresa de Smart TPV NOVA.</summary>
    /// <param name="empresaNovaId">Empresa activa en NOVA.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Bodegas pertenecientes a la empresa.</returns>
    Task<IReadOnlyList<BodegaInventarioDto>> ObtenerBodegasAsync(int empresaNovaId, CancellationToken cancellationToken);

    /// <summary>Busca productos de una bodega, incluyendo artículos sin existencia.</summary>
    /// <param name="empresaNovaId">Empresa activa en NOVA.</param>
    /// <param name="bodegaId">Bodega cuyos productos se consultan.</param>
    /// <param name="criterio">Texto opcional para filtrar por código o nombre.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Productos con su existencia y precio vigente.</returns>
    Task<IReadOnlyList<ArticuloInventarioDto>> ObtenerExistenciasAsync(int empresaNovaId, int bodegaId, string? criterio, CancellationToken cancellationToken);

    /// <summary>Obtiene un producto y su precio vigente dentro de la bodega indicada.</summary>
    /// <param name="empresaNovaId">Empresa activa en NOVA.</param>
    /// <param name="bodegaId">Bodega en la que se comprobará la existencia.</param>
    /// <param name="productoId">Producto solicitado.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>El producto encontrado o <see langword="null"/> si no pertenece a la empresa.</returns>
    Task<ArticuloInventarioDto?> ObtenerArticuloAsync(
        int empresaNovaId,
        int bodegaId,
        int productoId,
        CancellationToken cancellationToken);

    /// <summary>Registra un consumo interno mediante la operación oficial de salida de Smart TPV NOVA.</summary>
    /// <param name="solicitud">Empresa, bodega, producto, cantidad, usuario y orden de referencia.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Identificador de la salida generada en NOVA.</returns>
    Task<int> RegistrarSalidaAsync(
        RegistrarSalidaInventarioSolicitud solicitud,
        CancellationToken cancellationToken);

    /// <summary>Anula una salida y restituye sus existencias cuando una operación local no puede completarse.</summary>
    /// <param name="empresaNovaId">Empresa propietaria de la salida.</param>
    /// <param name="salidaId">Salida que debe compensarse.</param>
    /// <param name="usuarioId">Usuario responsable de la operación original.</param>
    /// <param name="motivo">Explicación auditable de la anulación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que representa la compensación.</returns>
    Task AnularSalidaAsync(
        int empresaNovaId,
        int salidaId,
        string usuarioId,
        string motivo,
        CancellationToken cancellationToken);
}
