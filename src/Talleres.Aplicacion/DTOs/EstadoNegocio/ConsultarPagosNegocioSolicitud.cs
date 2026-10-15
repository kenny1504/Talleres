using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.EstadoNegocio;

public enum SituacionPagoNegocio
{
    Todos,
    Vigentes,
    Anulados
}

public sealed class ConsultarPagosNegocioSolicitud
{
    public DateOnly? Desde { get; init; }

    public DateOnly? Hasta { get; init; }

    [Range(-840, 840)]
    public int DesfaseHorarioMinutos { get; init; }

    [EnumDataType(typeof(SituacionPagoNegocio))]
    public SituacionPagoNegocio Situacion { get; init; } = SituacionPagoNegocio.Todos;

    [StringLength(120)]
    public string? Busqueda { get; init; }

    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;

    [Range(1, 50)]
    public int TamanoPagina { get; init; } = 6;
}
