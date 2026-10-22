using System.ComponentModel.DataAnnotations;
using Talleres.Aplicacion.DTOs.Configuracion;
using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed class ConsultarExpedienteVehiculoPublicoSolicitud
{
    [Range(1, int.MaxValue)]
    public int Pagina { get; init; } = 1;
}

public sealed record VehiculoExpedientePublicoDto(string Placa, string Marca, string Modelo, int Anio, string? Color);
public sealed record CargoExpedientePublicoDto(string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal Subtotal);

public sealed record OrdenExpedientePublicoDto(
    string Numero, EstadoOrdenServicio Estado, DateTime FechaIngreso, DateTime? FechaAutorizacionClienteUtc,
    int? Kilometraje, string? MotivoIngreso, string? Diagnostico, bool EsOrdenCompartida,
    IReadOnlyCollection<CargoExpedientePublicoDto> Cargos, decimal Total);

public sealed record ExpedienteVehiculoPublicoDto(
    string NombreTaller, VehiculoExpedientePublicoDto Vehiculo,
    IReadOnlyCollection<OrdenExpedientePublicoDto> Ordenes,
    int Pagina, int TamanoPagina, int TotalOrdenes, ConfiguracionMonetariaDto ConfiguracionMonetaria);
