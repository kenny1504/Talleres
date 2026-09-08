namespace Talleres.Dominio.Entidades;

/// <summary>
/// Describe una fotografía almacenada como evidencia de una inspección.
/// </summary>
public sealed class EvidenciaInspeccion : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long RecepcionVehiculoId { get; set; }

    public required string ClaveObjeto { get; set; }

    public required string NombreArchivo { get; set; }

    public required string TipoContenido { get; set; }

    public long Longitud { get; set; }

    public DateTime FechaCargaUtc { get; set; }

    public RecepcionVehiculo RecepcionVehiculo { get; set; } = null!;
}
