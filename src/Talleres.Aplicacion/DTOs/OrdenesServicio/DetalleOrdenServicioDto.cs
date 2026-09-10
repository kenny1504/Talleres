using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed record DetalleOrdenServicioDto(
    long Id,
    TipoDetalleOrdenServicio Tipo,
    int? ProductoInventarioId,
    int? BodegaInventarioId,
    string? CodigoProducto,
    string Descripcion,
    string? UnidadMedida,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal,
    bool ExistenciaDescontada,
    DateTime FechaCreacion);
