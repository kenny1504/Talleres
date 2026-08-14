using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed class RegistrarRecepcionVehiculoSolicitud
{
    [Range(0, int.MaxValue)]
    public int Kilometraje { get; init; }

    [Range(0, 100)]
    public byte PorcentajeCombustible { get; init; }

    [Required, StringLength(2000, MinimumLength = 5)]
    public required string DescripcionEstado { get; init; }

    public bool DejaLlaves { get; init; }

    public bool DejaDocumentos { get; init; }

    public IReadOnlyCollection<RegistrarDanioVehiculoSolicitud> Danios { get; init; } = [];
}
