using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.CatalogosVehiculos;

public sealed class GuardarModeloVehiculoSolicitud
{
    [Range(1, long.MaxValue)]
    public long MarcaVehiculoId { get; init; }

    [Required, StringLength(80, MinimumLength = 1)]
    public required string Nombre { get; init; }

    public bool Activo { get; init; } = true;
}
