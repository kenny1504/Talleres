namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed record TallerSincronizadoDto(
    int EmpresaNovaId,
    string NombreLegal,
    string? NombreComercial,
    bool Activo);
