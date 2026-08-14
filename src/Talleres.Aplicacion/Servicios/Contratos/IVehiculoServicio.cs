using Talleres.Aplicacion.DTOs.Vehiculos;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona los vehículos asociados a los clientes de la empresa actual.
/// </summary>
public interface IVehiculoServicio
{
    /// <summary>
    /// Registra un vehículo para un cliente existente y valida la unicidad de la placa.
    /// </summary>
    /// <param name="solicitud">Datos de identificación y características del vehículo.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El vehículo creado con su identificador asignado.</returns>
    Task<VehiculoDto> CrearAsync(
        CrearVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un vehículo por su identificador dentro de la empresa actual.
    /// </summary>
    /// <param name="vehiculoId">Identificador interno del vehículo.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>La información del vehículo encontrado.</returns>
    Task<VehiculoDto> ObtenerPorIdAsync(
        long vehiculoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los vehículos visibles para la empresa actual ordenados por placa.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Colección de vehículos pertenecientes a la empresa.</returns>
    Task<IReadOnlyCollection<VehiculoDto>> ListarAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista los vehículos pertenecientes a un cliente de la empresa actual.
    /// </summary>
    /// <param name="clienteId">Identificador del cliente propietario.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Colección de vehículos del cliente.</returns>
    Task<IReadOnlyCollection<VehiculoDto>> ListarPorClienteAsync(
        long clienteId,
        CancellationToken cancellationToken = default);
}
