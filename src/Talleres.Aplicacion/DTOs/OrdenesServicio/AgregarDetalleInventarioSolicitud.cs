using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class AgregarDetalleInventarioSolicitud
{
    [Range(1, int.MaxValue)]
    public int ProductoId { get; init; }

    [Range(1, int.MaxValue)]
    public int BodegaId { get; init; }

    [Range(typeof(decimal), "0.0001", "999999999.9999")]
    public decimal Cantidad { get; init; }
}
