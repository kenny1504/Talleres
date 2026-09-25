using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.TecnicosTaller;

public sealed class GuardarTecnicoTallerSolicitud
{
    [Required, StringLength(150, MinimumLength = 2)]
    public required string Nombre { get; init; }

    public bool Activo { get; init; } = true;
}
