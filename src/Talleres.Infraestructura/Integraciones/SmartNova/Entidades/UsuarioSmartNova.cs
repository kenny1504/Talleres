namespace Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

public sealed class UsuarioSmartNova
{
    public string Id { get; set; } = string.Empty;

    public string? UserName { get; set; }

    public string? NormalizedUserName { get; set; }

    public string? PasswordHash { get; set; }

    public bool EsSuperUser { get; set; }

    public int? IdEmpresa { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public bool IsActive { get; set; }
}
