namespace Talleres.Dominio.Entidades;

/// <summary>
/// Describe una fotografía opcional que respalda el diagnóstico de una orden.
/// </summary>
public sealed class EvidenciaDiagnostico : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long OrdenServicioId { get; set; }

    public required string ClaveObjeto { get; set; }

    public required string NombreArchivo { get; set; }

    public required string TipoContenido { get; set; }

    public long Longitud { get; set; }

    public DateTime FechaCargaUtc { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;
}
