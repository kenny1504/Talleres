using Talleres.Aplicacion.DTOs.Autenticacion;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Aplica las reglas de acceso de Talleres sobre la identidad y las empresas de SMART TPV NOVA.
/// </summary>
public interface IAutenticacionServicio
{
    /// <summary>
    /// Autentica al usuario y selecciona su taller autorizado. Los superusuarios acceden a todos los talleres sincronizados activos.
    /// </summary>
    /// <param name="solicitud">Credenciales recibidas del formulario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Datos seguros de la sesión y del taller seleccionado.</returns>
    Task<SesionTallerDto> IniciarSesionAsync(
        InicioSesionSolicitud solicitud,
        CancellationToken cancellationToken);

    Task<SesionTallerDto> IniciarSesionExternaAsync(
        string proveedor,
        string claveProveedor,
        string? correo,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reconstruye la información visible de una sesión autenticada y revalida que usuario y taller continúen autorizados.
    /// </summary>
    /// <param name="usuarioId">Identificador confiable obtenido de la cookie.</param>
    /// <param name="empresaNovaId">Taller activo obtenido de la cookie.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Datos vigentes de la sesión.</returns>
    Task<SesionTallerDto> ObtenerSesionAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Cambia el taller activo después de comprobar nuevamente en NOVA que el usuario sigue siendo superusuario.
    /// </summary>
    /// <param name="usuarioId">Identificador confiable obtenido de la cookie.</param>
    /// <param name="empresaNovaId">Taller sincronizado solicitado.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Sesión actualizada para el taller seleccionado.</returns>
    Task<SesionTallerDto> SeleccionarTallerAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken);
}
