using System.ComponentModel.DataAnnotations;

namespace Talleres.Aplicacion.DTOs.Clientes;

public sealed class CrearClienteSolicitud
{
    [Required, StringLength(150, MinimumLength = 2)]
    public required string Nombre { get; init; }

    [Required, StringLength(30, MinimumLength = 3)]
    public required string DocumentoIdentidad { get; init; }

    [Required, StringLength(30, MinimumLength = 7)]
    public required string Telefono { get; init; }

    [EmailAddress, StringLength(150)]
    public string? Correo { get; init; }

    [StringLength(300)]
    public string? Direccion { get; init; }
}
