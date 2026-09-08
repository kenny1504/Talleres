namespace Talleres.Aplicacion.DTOs.CatalogosVehiculos;

public sealed record MarcaVehiculoDto(
    long Id,
    string Nombre,
    bool Activa,
    DateTime FechaCreacion);
