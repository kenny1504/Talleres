using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Autenticacion;

public sealed class InicioSesionSolicitud
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(256)]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(256)]
    public string Contrasena { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}
