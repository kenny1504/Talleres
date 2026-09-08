namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record EvidenciaInspeccionDto(
    long Id,
    string NombreArchivo,
    string TipoContenido,
    long Longitud,
    DateTime FechaCargaUtc);
