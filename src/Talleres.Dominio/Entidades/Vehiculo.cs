namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa un vehículo atendido por el taller.
/// </summary>
public sealed class Vehiculo : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long ClienteId { get; set; }

    public required string Placa { get; set; }

    public required string Marca { get; set; }

    public required string Modelo { get; set; }

    public int Anio { get; set; }

    public string? Color { get; set; }

    public string? NumeroVin { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public ICollection<OrdenServicio> OrdenesServicio { get; } = [];
}
