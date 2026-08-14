namespace Talleres.Aplicacion.DTOs.Vehiculos;

public sealed record VehiculoDto(
    long Id,
    long ClienteId,
    string NombreCliente,
    string Placa,
    string Marca,
    string Modelo,
    int Anio,
    string? Color,
    string? NumeroVin,
    bool Activo,
    DateTime FechaCreacion);
