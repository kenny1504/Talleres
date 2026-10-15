using System.ComponentModel.DataAnnotations;
using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record ResultadoRevisionOperativaDto
{
    [EnumDataType(typeof(ActividadRevisionOperativa))]
    public required ActividadRevisionOperativa Actividad { get; init; }

    public required bool Correcto { get; init; }
}
