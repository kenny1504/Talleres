using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class AsignarTecnicoOrdenSolicitud
{
    [Range(1, long.MaxValue)]
    public long TecnicoTallerId { get; init; }
}
