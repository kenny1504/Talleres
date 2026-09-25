using Talleres.Aplicacion.DTOs.TecnicosTaller;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>Administra los técnicos y el responsable predeterminado de la empresa activa.</summary>
public interface ITecnicoTallerServicio
{
    /// <summary>Lista los técnicos de la empresa, incluidos los inactivos para administración.</summary>
    /// <param name="cancellationToken">Token para cancelar la consulta.</param>
    /// <returns>Técnicos registrados en orden alfabético.</returns>
    Task<IReadOnlyCollection<TecnicoTallerDto>> ListarAsync(CancellationToken cancellationToken = default);

    /// <summary>Registra un técnico; el primero queda predeterminado y otro puede marcarse al crearlo.</summary>
    /// <param name="solicitud">Nombre e indicador de técnico predeterminado.</param>
    /// <param name="cancellationToken">Token para cancelar el registro.</param>
    /// <returns>El técnico registrado.</returns>
    Task<TecnicoTallerDto> CrearAsync(CrearTecnicoTallerSolicitud solicitud, CancellationToken cancellationToken = default);

    /// <summary>Corrige el nombre o estado de un técnico sin alterar las órdenes históricas.</summary>
    /// <param name="tecnicoId">Identificador del técnico.</param>
    /// <param name="solicitud">Nombre y estado deseados.</param>
    /// <param name="cancellationToken">Token para cancelar el cambio.</param>
    /// <returns>El técnico actualizado.</returns>
    Task<TecnicoTallerDto> ActualizarAsync(long tecnicoId, GuardarTecnicoTallerSolicitud solicitud, CancellationToken cancellationToken = default);

    /// <summary>Cambia de forma atómica el único técnico predeterminado de la empresa.</summary>
    /// <param name="tecnicoId">Identificador de un técnico activo.</param>
    /// <param name="cancellationToken">Token para cancelar el cambio.</param>
    /// <returns>El nuevo técnico predeterminado.</returns>
    Task<TecnicoTallerDto> EstablecerPredeterminadoAsync(long tecnicoId, CancellationToken cancellationToken = default);
}
