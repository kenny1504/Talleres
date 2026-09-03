using Talleres.Aplicacion.DTOs.Autenticacion;

namespace Talleres.Aplicacion.Abstracciones.Integraciones;

/// <summary>
/// Expone las operaciones de solo lectura necesarias para autenticar usuarios y consultar empresas de SMART TPV NOVA.
/// </summary>
public interface IIdentidadSmartNova
{
    /// <summary>
    /// Valida las credenciales con el hash de Identity almacenado por NOVA y devuelve únicamente datos seguros del usuario.
    /// </summary>
    /// <param name="usuario">Nombre de usuario registrado en NOVA.</param>
    /// <param name="contrasena">Contraseña en texto claro recibida durante el inicio de sesión.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Usuario activo cuando las credenciales son válidas; de lo contrario, nulo.</returns>
    Task<UsuarioSmartNovaDto?> AutenticarAsync(
        string usuario,
        string contrasena,
        CancellationToken cancellationToken);

    Task<UsuarioSmartNovaDto?> AutenticarProveedorAsync(
        string proveedor,
        string claveProveedor,
        string? correo,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene un usuario activo por su identificador para revalidar permisos sensibles.
    /// </summary>
    /// <param name="usuarioId">Identificador Identity de NOVA.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Datos seguros del usuario activo o nulo si dejó de estar disponible.</returns>
    Task<UsuarioSmartNovaDto?> ObtenerUsuarioActivoAsync(
        string usuarioId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene la información pública de las empresas indicadas directamente desde NOVA.
    /// </summary>
    /// <param name="empresaIds">Identificadores configurados como talleres en Talleres.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Empresas existentes, ordenadas por nombre.</returns>
    Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresAsync(
        IReadOnlyCollection<int> empresaIds,
        CancellationToken cancellationToken);
}
