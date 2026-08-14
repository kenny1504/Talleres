using Talleres.Dominio.Enumeraciones;

namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa el proceso de atención de un vehículo en el taller.
/// </summary>
public sealed class OrdenServicio : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public required string Numero { get; set; }

    public long ClienteId { get; set; }

    public long VehiculoId { get; set; }

    public EstadoOrdenServicio Estado { get; set; }

    public DateTime FechaIngreso { get; set; }

    public string? Observaciones { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public Vehiculo Vehiculo { get; set; } = null!;

    public RecepcionVehiculo? Recepcion { get; set; }

    public ICollection<HistorialOrdenServicio> Historial { get; } = [];
}
