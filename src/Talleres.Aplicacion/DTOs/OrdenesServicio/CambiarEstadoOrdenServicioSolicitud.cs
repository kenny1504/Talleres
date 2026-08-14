using System.ComponentModel.DataAnnotations;
using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class CambiarEstadoOrdenServicioSolicitud
{
    [EnumDataType(typeof(EstadoOrdenServicio))]
    public EstadoOrdenServicio Estado { get; init; }

    [Required, StringLength(300, MinimumLength = 3)]
    public required string Descripcion { get; init; }
}
