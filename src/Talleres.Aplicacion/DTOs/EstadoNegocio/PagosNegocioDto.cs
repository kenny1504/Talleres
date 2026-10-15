namespace Talleres.Aplicacion.DTOs.EstadoNegocio;

public sealed record PagoNegocioDto(
    long Id,
    long ClienteId,
    string NombreCliente,
    decimal Monto,
    DateTime FechaRegistroUtc);

public sealed record PagosNegocioDto(
    IReadOnlyList<PagoNegocioDto> Pagos,
    int TotalRegistros,
    int Pagina,
    int TamanoPagina,
    int TotalPaginas);
