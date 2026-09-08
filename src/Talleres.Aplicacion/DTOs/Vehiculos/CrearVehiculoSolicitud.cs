using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Vehiculos;

public sealed class CrearVehiculoSolicitud
{
    [Range(1, long.MaxValue)]
    public long ClienteId { get; init; }

    [Required, StringLength(15, MinimumLength = 2)]
    public required string Placa { get; init; }

    [Range(1, long.MaxValue)]
    public long ModeloVehiculoId { get; init; }

    [Range(1900, 2100)]
    public int Anio { get; init; }

    [StringLength(40)]
    public string? Color { get; init; }

    [StringLength(50)]
    public string? NumeroVin { get; init; }
}
