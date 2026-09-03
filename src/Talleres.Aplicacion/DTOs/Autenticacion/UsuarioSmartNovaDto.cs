namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed record UsuarioSmartNovaDto(
    string Id,
    string Usuario,
    string Nombre,
    bool EsSuperUsuario,
    int? EmpresaId);
