using Talleres.Dominio.Enumeraciones;

namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa un producto, servicio o cargo incorporado a una orden.
/// </summary>
public sealed class DetalleOrdenServicio : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long OrdenServicioId { get; set; }

    public TipoDetalleOrdenServicio Tipo { get; set; }

    public int? ProductoInventarioId { get; set; }

    public int? BodegaInventarioId { get; set; }

    public int? SalidaInventarioId { get; set; }

    public string? CodigoProducto { get; set; }

    public required string Descripcion { get; set; }

    public string? UnidadMedida { get; set; }

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public bool ExistenciaDescontada { get; set; }

    public DateTime FechaCreacion { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;
}
