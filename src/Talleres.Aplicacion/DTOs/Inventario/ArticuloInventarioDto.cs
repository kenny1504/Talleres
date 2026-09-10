namespace Talleres.Aplicacion.DTOs.Inventario;

public sealed record ArticuloInventarioDto(
    int ProductoId,
    string Codigo,
    string Nombre,
    string UnidadMedida,
    decimal Existencia,
    decimal? PrecioUnitario);
