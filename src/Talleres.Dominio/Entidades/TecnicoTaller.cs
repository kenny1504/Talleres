namespace Talleres.Dominio.Entidades;

/// <summary>Persona del taller que puede quedar a cargo de una orden de servicio.</summary>
public sealed class TecnicoTaller : IEntidadEmpresa
{
    public long Id { get; set; }
    public long EmpresaId { get; set; }
    public required string Nombre { get; set; }
    public bool Activo { get; set; } = true;
    public bool EsPredeterminado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public ICollection<OrdenServicio> OrdenesServicio { get; } = [];
}
