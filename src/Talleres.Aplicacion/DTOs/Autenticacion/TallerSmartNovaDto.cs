namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed record TallerSmartNovaDto(
    int Id,
    string NombreLegal,
    string? NombreComercial,
    int PrefijoTelefono,
    string Telefono,
    string? Ruc,
    string? Correo,
    string? Direccion,
    string? Ciudad,
    string? Barrio,
    string? Calle,
    string? Logo,
    TimeSpan HoraApertura,
    TimeSpan HoraCierre);
