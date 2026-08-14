using System.ComponentModel.DataAnnotations;
using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.Recepciones;

public sealed class RegistrarDanioVehiculoSolicitud
{
    [EnumDataType(typeof(ZonaVehiculo))]
    public ZonaVehiculo Zona { get; init; }

    [EnumDataType(typeof(TipoDanioVehiculo))]
    public TipoDanioVehiculo Tipo { get; init; }

    [EnumDataType(typeof(SeveridadDanioVehiculo))]
    public SeveridadDanioVehiculo Severidad { get; init; }

    [StringLength(500)]
    public string? Observacion { get; init; }
}
