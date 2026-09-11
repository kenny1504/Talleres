namespace Talleres.Aplicacion.DTOs.Clientes;

public sealed record ClienteDto(
    long Id,
    string Nombre,
    string Telefono,
    string? Direccion,
    bool Activo,
    DateTime FechaCreacion);
