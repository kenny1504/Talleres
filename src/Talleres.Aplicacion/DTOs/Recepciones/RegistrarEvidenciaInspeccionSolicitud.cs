namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record RegistrarEvidenciaInspeccionSolicitud(
    string NombreArchivo,
    string TipoContenido,
    long Longitud,
    Stream Contenido);
