using Talleres.Aplicacion.DTOs.Recepciones;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona las fotografías de evidencia vinculadas con una inspección vehicular.
/// </summary>
public interface IEvidenciaInspeccionServicio
{
    /// <summary>
    /// Valida, almacena y registra una colección de imágenes para una recepción existente.
    /// </summary>
    /// <param name="ordenServicioId">Orden cuya recepción recibirá las evidencias.</param>
    /// <param name="solicitudes">Imágenes validadas estructuralmente por el controlador.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Metadatos de las evidencias creadas.</returns>
    Task<IReadOnlyCollection<EvidenciaInspeccionDto>> RegistrarAsync(
        long ordenServicioId,
        IReadOnlyCollection<RegistrarEvidenciaInspeccionSolicitud> solicitudes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la dirección pública después de validar empresa, orden y evidencia.
    /// </summary>
    /// <param name="ordenServicioId">Orden propietaria de la recepción.</param>
    /// <param name="evidenciaId">Evidencia solicitada.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Dirección pública permanente de solo lectura.</returns>
    Task<Uri> CrearDireccionLecturaAsync(
        long ordenServicioId,
        long evidenciaId,
        CancellationToken cancellationToken = default);
}
