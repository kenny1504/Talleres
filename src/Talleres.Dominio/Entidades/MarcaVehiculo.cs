namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa una marca de vehículo registrada para uso interno de una empresa.
/// </summary>
public sealed class MarcaVehiculo : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public required string Nombre { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public ICollection<ModeloVehiculo> Modelos { get; } = [];
}
