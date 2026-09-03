namespace Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

public sealed class EmpresaSmartNova
{
    public int Id { get; set; }

    public string NombreLegal { get; set; } = string.Empty;

    public string? NombreComercial { get; set; }

    public int PrefijoTelefono { get; set; }

    public decimal Telefono { get; set; }

    public string? Ruc { get; set; }

    public string? Correo { get; set; }

    public string? Dirreccion { get; set; }

    public string? Ciudad { get; set; }

    public string? Barrio { get; set; }

    public string? Calle { get; set; }

    public string? Logo { get; set; }

    public TimeSpan HoraApertura { get; set; }

    public TimeSpan HoraCierre { get; set; }
}
