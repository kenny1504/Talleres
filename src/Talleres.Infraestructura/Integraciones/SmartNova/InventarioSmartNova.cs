using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Inventario;

namespace Talleres.Infraestructura.Integraciones.SmartNova;

public sealed class InventarioSmartNova(SmartNovaDbContext contexto) : IInventarioSmartNova
{
    public async Task<IReadOnlyList<BodegaInventarioDto>> ObtenerBodegasAsync(int empresaNovaId, CancellationToken cancellationToken)
    {
        var filas = await contexto.Database.SqlQueryRaw<FilaBodega>(
            "EXEC dbo.sp_GetBodegasByEmpresa @EmpresaId={0}", empresaNovaId).ToListAsync(cancellationToken);
        return filas.Select(item => new BodegaInventarioDto(item.Id, item.Nombre, item.EsPrincipal)).ToList();
    }

    public async Task<IReadOnlyList<ArticuloInventarioDto>> ObtenerExistenciasAsync(int empresaNovaId, int bodegaId, string? criterio, CancellationToken cancellationToken)
    {
        var filas = await contexto.Database.SqlQueryRaw<FilaArticulo>(
            "EXEC dbo.sp_BuscarProductosParaSalida @EmpresaId={0}, @BodegaId={1}, @Criterio={2}, @Top={3}",
            empresaNovaId, bodegaId, criterio is null ? DBNull.Value : criterio, 200).ToListAsync(cancellationToken);
        return filas.Where(item => item.StockDisponible > 0)
            .Select(item => new ArticuloInventarioDto(item.ProductoId, item.Codigo, item.Nombre, item.UnidadMedida, Convert.ToDecimal(item.StockDisponible), item.CostoUnitario is null ? null : Convert.ToDecimal(item.CostoUnitario.Value)))
            .ToList();
    }

    private sealed class FilaBodega { public int Id { get; set; } public string Nombre { get; set; } = string.Empty; public bool EsPrincipal { get; set; } }
    private sealed class FilaArticulo { public int ProductoId { get; set; } public string Codigo { get; set; } = string.Empty; public string Nombre { get; set; } = string.Empty; public string UnidadMedida { get; set; } = string.Empty; public double StockDisponible { get; set; } public double? CostoUnitario { get; set; } }
}
