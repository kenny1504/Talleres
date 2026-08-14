namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa al propietario o responsable de uno o más vehículos.
/// </summary>
public sealed class Cliente : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public required string Nombre { get; set; }

    public required string DocumentoIdentidad { get; set; }

    public required string Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }

    public ICollection<Vehiculo> Vehiculos { get; } = [];

    public ICollection<OrdenServicio> OrdenesServicio { get; } = [];
}
