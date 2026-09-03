using Talleres.Aplicacion.DTOs.Inventario;

namespace Talleres.Aplicacion.Abstracciones.Integraciones;

public interface IInventarioSmartNova
{
    Task<IReadOnlyList<BodegaInventarioDto>> ObtenerBodegasAsync(int empresaNovaId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ArticuloInventarioDto>> ObtenerExistenciasAsync(int empresaNovaId, int bodegaId, string? criterio, CancellationToken cancellationToken);
}
