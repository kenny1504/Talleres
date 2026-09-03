namespace Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

public sealed class InicioSesionExternoSmartNova
{
    public string LoginProvider { get; set; } = string.Empty;
    public string ProviderKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
