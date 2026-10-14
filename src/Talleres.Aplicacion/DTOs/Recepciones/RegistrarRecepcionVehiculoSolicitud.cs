using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed class RegistrarRecepcionVehiculoSolicitud
{
    [Range(0, int.MaxValue)]
    public int Kilometraje { get; init; }

    [Range(0, 100)]
    public byte PorcentajeCombustible { get; init; }

    [StringLength(2000)]
    public string? DescripcionEstado { get; init; }

    public bool DejaLlaves { get; init; }

    public bool DejaDocumentos { get; init; }

    public bool DejaLlantaRepuesto { get; init; }

    public bool DejaTrianguloSeguridad { get; init; }

    public bool DejaGata { get; init; }

    public bool DejaLlaveRuedas { get; init; }

    public bool DejaGanchoRemolque { get; init; }

    public bool DejaExtintor { get; init; }

    public bool DejaBotiquin { get; init; }

    public bool DejaChalecoReflectante { get; init; }

    [StringLength(500)]
    public string? OtrosElementos { get; init; }

    public bool AceptaPruebaRuta { get; init; }

    public IReadOnlyCollection<RegistrarDanioVehiculoSolicitud> Danios { get; init; } = [];
}
