namespace Talleres.Dominio.Entidades;

/// <summary>
/// Conserva el estado físico y operativo informado al recibir un vehículo.
/// </summary>
public sealed class RecepcionVehiculo : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long OrdenServicioId { get; set; }

    public int Kilometraje { get; set; }

    public byte PorcentajeCombustible { get; set; }

    public required string DescripcionEstado { get; set; }

    public string? ObservacionRevisionOperativa { get; set; }

    public ICollection<ResultadoRevisionOperativa> RevisionOperativa { get; } = [];

    public bool DejaLlaves { get; set; }

    public bool DejaDocumentos { get; set; }

    public bool DejaLlantaRepuesto { get; set; }

    public bool DejaTrianguloSeguridad { get; set; }

    public bool DejaGata { get; set; }

    public bool DejaLlaveRuedas { get; set; }

    public bool DejaGanchoRemolque { get; set; }

    public bool DejaExtintor { get; set; }

    public bool DejaBotiquin { get; set; }

    public bool DejaChalecoReflectante { get; set; }

    public bool DejaPasacorrientes { get; set; }

    public bool DejaCopaRueda { get; set; }

    public bool DejaAlfombras { get; set; }

    public string? OtrosElementos { get; set; }

    public bool AceptaPruebaRuta { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;

    public ICollection<DanioVehiculo> Danios { get; } = [];

    public ICollection<EvidenciaInspeccion> Evidencias { get; } = [];
}
