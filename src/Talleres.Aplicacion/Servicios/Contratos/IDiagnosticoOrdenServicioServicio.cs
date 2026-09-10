using Talleres.Aplicacion.DTOs.OrdenesServicio;

namespace Talleres.Aplicacion.Servicios.Contratos;

/// <summary>
/// Gestiona el diagnóstico de una orden y su consulta segura por el cliente.
/// </summary>
public interface IDiagnosticoOrdenServicioServicio
{
    /// <summary>Obtiene el diagnóstico de una orden perteneciente a la empresa actual.</summary>
    Task<DiagnosticoOrdenServicioDto> ObtenerAsync(long ordenServicioId, CancellationToken cancellationToken = default);

    /// <summary>Guarda el diagnóstico y crea, cuando sea necesario, su token público no predecible.</summary>
    Task<DiagnosticoOrdenServicioDto> GuardarAsync(long ordenServicioId, GuardarDiagnosticoOrdenServicioSolicitud solicitud, CancellationToken cancellationToken = default);

    /// <summary>Registra fotografías opcionales vinculadas con el diagnóstico.</summary>
    Task<DiagnosticoOrdenServicioDto> RegistrarEvidenciasAsync(long ordenServicioId, IReadOnlyCollection<RegistrarEvidenciaDiagnosticoSolicitud> solicitudes, CancellationToken cancellationToken = default);

    /// <summary>Elimina una fotografía del diagnóstico dentro de la empresa actual.</summary>
    Task EliminarEvidenciaAsync(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken = default);

    /// <summary>Crea la dirección de lectura de una evidencia después de validar la orden y empresa.</summary>
    Task<Uri> CrearDireccionEvidenciaAsync(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken = default);

    /// <summary>Obtiene la vista pública limitada de una orden mediante su token.</summary>
    Task<OrdenServicioPublicaDto> ObtenerPublicaAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Registra de forma idempotente la autorización del cliente mediante el token público.</summary>
    Task<OrdenServicioPublicaDto> AutorizarPublicaAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Crea la dirección de lectura de una evidencia usando exclusivamente el token público.</summary>
    Task<Uri> CrearDireccionEvidenciaPublicaAsync(string token, long evidenciaId, CancellationToken cancellationToken = default);

    /// <summary>Crea la dirección de lectura de una fotografía de inspección vinculada con el token público.</summary>
    Task<Uri> CrearDireccionEvidenciaInspeccionPublicaAsync(string token, long evidenciaId, CancellationToken cancellationToken = default);
}
