using Talleres.Aplicacion.DTOs.CatalogosVehiculos;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona las marcas y sus modelos dentro del catálogo privado de la empresa actual.
/// </summary>
public interface ICatalogoVehiculoServicio
{
    /// <summary>Lista las marcas de la empresa actual.</summary>
    /// <param name="incluirInactivas">Indica si deben incluirse marcas inactivas.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Las marcas ordenadas por nombre.</returns>
    Task<IReadOnlyCollection<MarcaVehiculoDto>> ListarMarcasAsync(
        bool incluirInactivas = false,
        CancellationToken cancellationToken = default);

    /// <summary>Registra una marca única para la empresa actual.</summary>
    /// <param name="solicitud">Nombre y estado inicial de la marca.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La marca creada.</returns>
    Task<MarcaVehiculoDto> CrearMarcaAsync(
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza una marca perteneciente a la empresa actual.</summary>
    /// <param name="marcaId">Identificador de la marca.</param>
    /// <param name="solicitud">Nombre y estado que se guardarán.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La marca actualizada.</returns>
    Task<MarcaVehiculoDto> ActualizarMarcaAsync(
        long marcaId,
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>Lista los modelos de una marca de la empresa actual.</summary>
    /// <param name="marcaId">Identificador opcional para filtrar por marca.</param>
    /// <param name="incluirInactivos">Indica si deben incluirse modelos inactivos.</param>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Los modelos ordenados por marca y nombre.</returns>
    Task<IReadOnlyCollection<ModeloVehiculoDto>> ListarModelosAsync(
        long? marcaId = null,
        bool incluirInactivos = false,
        CancellationToken cancellationToken = default);

    /// <summary>Registra un modelo en una marca de la empresa actual.</summary>
    /// <param name="solicitud">Marca, nombre y estado inicial del modelo.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El modelo creado.</returns>
    Task<ModeloVehiculoDto> CrearModeloAsync(
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);

    /// <summary>Actualiza un modelo y valida su pertenencia a la marca indicada.</summary>
    /// <param name="modeloId">Identificador del modelo.</param>
    /// <param name="solicitud">Marca, nombre y estado que se guardarán.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El modelo actualizado.</returns>
    Task<ModeloVehiculoDto> ActualizarModeloAsync(
        long modeloId,
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
