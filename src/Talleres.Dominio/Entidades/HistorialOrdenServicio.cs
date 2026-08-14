using Talleres.Dominio.Enumeraciones;

namespace Talleres.Dominio.Entidades;

/// <summary>
/// Registra cada transición de estado realizada sobre una orden de servicio.
/// </summary>
public sealed class HistorialOrdenServicio : IEntidadEmpresa
{
    public long Id { get; set; }

    public long EmpresaId { get; set; }

    public long OrdenServicioId { get; set; }

    public EstadoOrdenServicio? EstadoAnterior { get; set; }

    public EstadoOrdenServicio EstadoNuevo { get; set; }

    public required string Descripcion { get; set; }

    public DateTime Fecha { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;
}
