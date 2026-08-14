using Talleres.Aplicacion.DTOs.Clientes;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona la información de los clientes de la empresa actual.
/// </summary>
public interface IClienteServicio
{
    /// <summary>
    /// Registra un cliente después de verificar que su documento no esté duplicado.
    /// </summary>
    /// <param name="solicitud">Datos personales y de contacto del cliente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El cliente creado con su identificador asignado.</returns>
    Task<ClienteDto> CrearAsync(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza los datos de un cliente existente en la empresa actual.
    /// </summary>
    /// <param name="clienteId">Identificador interno del cliente.</param>
    /// <param name="solicitud">Datos que reemplazarán la información vigente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El cliente con la información actualizada.</returns>
    Task<ClienteDto> ActualizarAsync(
        long clienteId,
        ActualizarClienteSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un cliente por su identificador dentro de la empresa actual.
    /// </summary>
    /// <param name="clienteId">Identificador interno del cliente.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>La información del cliente encontrado.</returns>
    Task<ClienteDto> ObtenerPorIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los clientes de la empresa actual ordenados por nombre.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Colección de clientes visibles para la empresa.</returns>
    Task<IReadOnlyCollection<ClienteDto>> ListarAsync(
        CancellationToken cancellationToken = default);
}
