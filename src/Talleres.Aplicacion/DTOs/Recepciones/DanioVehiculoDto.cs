using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed record DanioVehiculoDto(
    long Id,
    ZonaVehiculo Zona,
    TipoDanioVehiculo Tipo,
    SeveridadDanioVehiculo Severidad,
    string? Observacion);
