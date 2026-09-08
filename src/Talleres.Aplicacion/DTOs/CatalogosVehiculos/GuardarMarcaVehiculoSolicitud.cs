using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.CatalogosVehiculos;

public sealed class GuardarMarcaVehiculoSolicitud
{
    [Required, StringLength(80, MinimumLength = 2)]
    public required string Nombre { get; init; }

    public bool Activa { get; init; } = true;
}
