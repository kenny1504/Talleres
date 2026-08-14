using Talleres.Dominio.Enumeraciones;

namespace Talleres.Dominio.Entidades;

/// <summary>
/// Representa un hallazgo exterior marcado durante la recepción del vehículo.
/// </summary>
public sealed class DanioVehiculo : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long RecepcionVehiculoId { get; set; }

    public ZonaVehiculo Zona { get; set; }

    public TipoDanioVehiculo Tipo { get; set; }

    public SeveridadDanioVehiculo Severidad { get; set; }

    public string? Observacion { get; set; }

    public RecepcionVehiculo RecepcionVehiculo { get; set; } = null!;
}
