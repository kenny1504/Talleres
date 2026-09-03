using Talleres.Aplicacion.DTOs.Autenticacion;

namespace Talleres.Aplicacion.Abstracciones.Integraciones;

/// <summary>Administra el catálogo local de empresas de NOVA habilitadas como talleres.</summary>
public interface ITalleresSincronizadosServicio
{
    /// <summary>Obtiene las empresas configuradas, incluyendo las retiradas, para auditoría administrativa.</summary>
    Task<IReadOnlyList<TallerSincronizadoDto>> ListarAsync(string usuarioId, CancellationToken cancellationToken);

    /// <summary>Agrega o reactiva una empresa existente en NOVA como taller sincronizado.</summary>
    Task<TallerSincronizadoDto> AgregarAsync(string usuarioId, int empresaNovaId, CancellationToken cancellationToken);

    /// <summary>Retira una empresa del catálogo sin borrar el registro de configuración.</summary>
    Task RetirarAsync(string usuarioId, int empresaNovaId, CancellationToken cancellationToken);
}
