namespace Talleres.Aplicacion.DTOs.Clientes;

public sealed record ClienteDto(
    long Id,
    string Nombre,
    string DocumentoIdentidad,
    string Telefono,
    string? Correo,
    string? Direccion,
    bool Activo,
    DateTime FechaCreacion);
