namespace Talleres.Dominio.Enumeraciones;

/// <summary>
/// Indica la etapa actual de una orden dentro del flujo del taller.
/// </summary>
public enum EstadoOrdenServicio
{
    Recepcion = 1,
    Diagnostico = 2,
    Cotizacion = 3,
    PendienteAprobacion = 4,
    PreparacionReparacion = 5,
    Reparacion = 6,
    ControlCalidad = 7,
    ListaParaEntrega = 8,
    Entregada = 9,
    Cerrada = 10,
    Cancelada = 11
}
