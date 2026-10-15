using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.EstadoNegocio;

public enum AgrupacionCobrosNegocio
{
    Dia,
    Semana,
    Mes
}

public sealed class ConsultarEstadoNegocioSolicitud
{
    [Required]
    public DateOnly? Desde { get; init; }

    [Required]
    public DateOnly? Hasta { get; init; }

    // Coincide con getTimezoneOffset: minutos que se suman a la hora local para obtener UTC.
    [Range(-840, 840)]
    public int DesfaseHorarioMinutos { get; init; }

    [EnumDataType(typeof(AgrupacionCobrosNegocio))]
    public AgrupacionCobrosNegocio Agrupacion { get; init; } = AgrupacionCobrosNegocio.Dia;
}
