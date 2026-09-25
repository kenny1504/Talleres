using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.TecnicosTaller;

public sealed class CrearTecnicoTallerSolicitud
{
    [Required, StringLength(150, MinimumLength = 2)]
    public required string Nombre { get; init; }

    public bool EsPredeterminado { get; init; }
}
