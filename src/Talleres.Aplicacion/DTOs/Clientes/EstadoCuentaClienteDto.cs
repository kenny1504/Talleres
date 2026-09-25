using Talleres.Dominio.Enumeraciones;

namespace Talleres.Aplicacion.DTOs.Clientes;

public sealed record OrdenEstadoCuentaClienteDto(
    long Id,
    string Numero,
    EstadoOrdenServicio Estado,
    DateTime FechaIngreso,
    decimal Total);

public sealed record PagoClienteDto(
    long Id,
    decimal Monto,
    DateTime FechaRegistroUtc,
    string? FormaPago,
    string? Referencia,
    DateTime? FechaAnulacionUtc);

public sealed record EstadoCuentaClienteDto(
    long ClienteId,
    string NombreCliente,
    decimal TotalOrdenes,
    decimal TotalPagos,
    decimal Saldo,
    IReadOnlyCollection<OrdenEstadoCuentaClienteDto> Ordenes,
    IReadOnlyCollection<PagoClienteDto> Pagos);
