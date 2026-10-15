using Talleres.Dominio.Enumeraciones;

namespace Talleres.Dominio.Entidades;

public sealed class ResultadoRevisionOperativa : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long RecepcionVehiculoId { get; set; }

    public ActividadRevisionOperativa Actividad { get; set; }

    public bool Correcto { get; set; }

    public RecepcionVehiculo RecepcionVehiculo { get; set; } = null!;
}
