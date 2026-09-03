using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed class ConfigurarTallerSolicitud
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar una empresa válida.")]
    public int EmpresaNovaId { get; set; }
}
