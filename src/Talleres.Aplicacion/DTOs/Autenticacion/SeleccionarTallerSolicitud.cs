using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed class SeleccionarTallerSolicitud
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un taller válido.")]
    public int EmpresaNovaId { get; set; }
}
