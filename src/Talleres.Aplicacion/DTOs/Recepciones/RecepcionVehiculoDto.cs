namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record RecepcionVehiculoDto(
    long Id,
    long OrdenServicioId,
    int Kilometraje,
    byte PorcentajeCombustible,
    string DescripcionEstado,
    bool DejaLlaves,
    bool DejaDocumentos,
    DateTime FechaRecepcion,
    IReadOnlyCollection<DanioVehiculoDto> Danios);
