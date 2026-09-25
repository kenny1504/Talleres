namespace Talleres.Aplicacion.DTOs.TecnicosTaller;

public sealed record TecnicoTallerDto(
    long Id,
    string Nombre,
    bool Activo,
    bool EsPredeterminado);
