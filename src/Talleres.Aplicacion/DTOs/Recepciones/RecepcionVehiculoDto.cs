namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record RecepcionVehiculoDto(
    long Id,
    long OrdenServicioId,
    int Kilometraje,
    byte PorcentajeCombustible,
    string DescripcionEstado,
    bool DejaLlaves,
    bool DejaDocumentos,
    DateTime FechaRecepcion,
    IReadOnlyCollection<DanioVehiculoDto> Danios,
    IReadOnlyCollection<EvidenciaInspeccionDto> Evidencias)
{
    public IReadOnlyCollection<ResultadoRevisionOperativaDto> RevisionOperativa { get; init; } = [];
    public string? ObservacionRevisionOperativa { get; init; }
    public bool DejaLlantaRepuesto { get; init; }
    public bool DejaTrianguloSeguridad { get; init; }
    public bool DejaGata { get; init; }
    public bool DejaLlaveRuedas { get; init; }
    public bool DejaGanchoRemolque { get; init; }
    public bool DejaExtintor { get; init; }
    public bool DejaBotiquin { get; init; }
    public bool DejaChalecoReflectante { get; init; }
    public bool DejaPasacorrientes { get; init; }
    public bool DejaCopaRueda { get; init; }
    public bool DejaAlfombras { get; init; }
    public string? OtrosElementos { get; init; }
    public bool AceptaPruebaRuta { get; init; }
}
