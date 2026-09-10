namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed record ResumenDetallesOrdenServicioDto(
    IReadOnlyCollection<DetalleOrdenServicioDto> Detalles,
    decimal Total);
