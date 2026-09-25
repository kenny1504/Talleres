using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Clientes;

public sealed class RegistrarPagoClienteSolicitud
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99")]
    public decimal Monto { get; init; }

    [StringLength(60)]
    public string? FormaPago { get; init; }

    [StringLength(120)]
    public string? Referencia { get; init; }
}
