namespace Talleres.Aplicacion.Abstracciones.Integraciones;

/// <summary>
/// Almacena y permite consultar evidencias fotográficas de una inspección.
/// </summary>
public interface IAlmacenamientoEvidencias
{
    /// <summary>
    /// Guarda una evidencia y devuelve la clave asignada al objeto.
    /// </summary>
    /// <param name="empresaId">Empresa propietaria de la evidencia.</param>
    /// <param name="recepcionVehiculoId">Recepción a la que pertenece.</param>
    /// <param name="nombreArchivo">Nombre original seguro para obtener la extensión.</param>
    /// <param name="tipoContenido">Tipo MIME validado de la imagen.</param>
    /// <param name="contenido">Contenido de la imagen.</param>
    /// <param name="cancellationToken">Token para cancelar la carga.</param>
    /// <returns>Clave del objeto almacenado.</returns>
    Task<string> GuardarAsync(
        long empresaId,
        long recepcionVehiculoId,
        string nombreArchivo,
        string tipoContenido,
        Stream contenido,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea la dirección pública de lectura de una evidencia.
    /// </summary>
    /// <param name="claveObjeto">Clave del objeto.</param>
    /// <param name="nombreArchivo">Nombre que se propondrá al navegador.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Dirección pública permanente del objeto.</returns>
    Task<Uri> CrearDireccionLecturaAsync(
        string claveObjeto,
        string nombreArchivo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un objeto cargado cuando no fue posible registrar sus metadatos.
    /// </summary>
    /// <param name="claveObjeto">Clave del objeto.</param>
    /// <param name="cancellationToken">Token para cancelar la eliminación.</param>
    Task EliminarAsync(
        string claveObjeto,
        CancellationToken cancellationToken = default);
}
