namespace Talleres.Aplicacion.DTOs.CatalogosVehiculos;

public sealed record ModeloVehiculoDto(
    long Id,
    long MarcaVehiculoId,
    string NombreMarca,
    string Nombre,
    bool Activo,
    DateTime FechaCreacion);
