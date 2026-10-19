namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa al propietario o responsable de uno o más vehículos.
/// </summary>
public sealed class Cliente : IEntidadEmpresa
{
    private string telefono = string.Empty;

    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public required string Nombre { get; set; }

    public required string Telefono
    {
        get => telefono;
        set
        {
            telefono = value;
            TelefonoNormalizado = NormalizarTelefono(value);
        }
    }

    public string TelefonoNormalizado { get; private set; } = string.Empty;

    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public ICollection<Vehiculo> Vehiculos { get; } = [];

    public ICollection<OrdenServicio> OrdenesServicio { get; } = [];

    public ICollection<PagoCliente> Pagos { get; } = [];

    /// <summary>Quita los separadores de presentación sin cambiar el prefijo del número.</summary>
    public static string NormalizarTelefono(string valor) => valor
        .Replace(" ", string.Empty)
        .Replace("-", string.Empty)
        .Replace("(", string.Empty)
        .Replace(")", string.Empty)
        .Replace("+", string.Empty)
        .Replace(".", string.Empty)
        .Replace("\t", string.Empty)
        .Replace("\r", string.Empty)
        .Replace("\n", string.Empty)
        .Replace("\u00a0", string.Empty);
}
