using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class AgregarDetalleManualSolicitud
{
    [Required, StringLength(300, MinimumLength = 2)]
    public required string Descripcion { get; init; }

    [StringLength(50)]
    public string? UnidadMedida { get; init; }

    [Range(typeof(decimal), "0.0001", "999999999.9999")]
    public decimal Cantidad { get; init; }

    [Range(typeof(decimal), "0", "999999999.9999")]
    public decimal PrecioUnitario { get; init; }
}
