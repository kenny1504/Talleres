namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed record SesionTallerDto(
    string UsuarioId,
    string Usuario,
    string NombreUsuario,
    bool EsSuperUsuario,
    TallerSmartNovaDto Taller,
    IReadOnlyList<TallerSmartNovaDto> TalleresDisponibles);
