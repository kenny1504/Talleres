using Talleres.Aplicacion.DTOs.Clientes;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona la información de los clientes de la empresa actual.
/// </summary>
public interface IClienteServicio
{
    /// <summary>Elimina un cliente de la empresa activa si no tiene vehículos, órdenes ni pagos, incluidos los anulados.</summary>
    /// <param name="clienteId">Identificador del cliente que se eliminará.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que finaliza después de eliminar el cliente.</returns>
    /// <exception cref="Talleres.Dominio.Excepciones.ReglaNegocioException">El cliente conserva registros asociados.</exception>
    Task EliminarAsync(long clienteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registra un cliente si su teléfono no pertenece a otro cliente de la empresa,
    /// incluidos los inactivos. La comparación ignora separadores de presentación.
    /// </summary>
    /// <param name="solicitud">Datos personales y de contacto del cliente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El cliente creado con su identificador asignado.</returns>
    /// <exception cref="Talleres.Dominio.Excepciones.ReglaNegocioException">El teléfono ya está registrado.</exception>
    Task<ClienteDto> CrearAsync(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza un cliente de la empresa actual sin permitir teléfonos de otros clientes.
    /// Permite conservar el número propio aunque cambie su formato.
    /// </summary>
    /// <param name="clienteId">Identificador interno del cliente.</param>
    /// <param name="solicitud">Datos que reemplazarán la información vigente.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El cliente con la información actualizada.</returns>
    /// <exception cref="Talleres.Dominio.Excepciones.ReglaNegocioException">El teléfono pertenece a otro cliente.</exception>
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
