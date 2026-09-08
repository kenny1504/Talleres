namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa un modelo perteneciente a una marca del catálogo de una empresa.
/// </summary>
public sealed class ModeloVehiculo : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long MarcaVehiculoId { get; set; }

    public required string Nombre { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public MarcaVehiculo Marca { get; set; } = null!;

    public ICollection<Vehiculo> Vehiculos { get; } = [];
}
