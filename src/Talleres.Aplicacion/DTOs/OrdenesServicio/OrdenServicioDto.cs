using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.OrdenesServicio;

public sealed record OrdenServicioDto(
    long Id,
    string Numero,
    long ClienteId,
    string NombreCliente,
    long VehiculoId,
    string PlacaVehiculo,
    EstadoOrdenServicio Estado,
    DateTime FechaIngreso,
    string? Observaciones,
    bool TieneRecepcion);
