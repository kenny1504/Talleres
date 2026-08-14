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

    public bool DejaLlaves { get; set; }

    public bool DejaDocumentos { get; set; }

    public DateTime FechaRecepcion { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;

    public ICollection<DanioVehiculo> Danios { get; } = [];
}
