using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class CrearOrdenServicioSolicitud
{
    [Range(1, long.MaxValue)]
    public long ClienteId { get; init; }

    [Range(1, long.MaxValue)]
    public long VehiculoId { get; init; }

    [StringLength(1000)]
    public string? Observaciones { get; init; }
}
