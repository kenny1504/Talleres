namespace Talleres.Aplicacion.DTOs.EstadoNegocio;

public enum SituacionCuentaNegocio
{
    Pendiente,
    Liquidada,
    SaldoAFavor
}

public sealed record CuentaNegocioDto(
    long ClienteId,
    string Nombre,
    decimal ImporteTrabajos,
    decimal Abonos,
    decimal Saldo,
    SituacionCuentaNegocio Situacion);

public sealed record ClientePrincipalNegocioDto(
    long ClienteId,
    string Nombre,
    int Ordenes,
    decimal Importe);

public sealed record VehiculoFrecuenteNegocioDto(
    long VehiculoId,
    string Placa,
    string Marca,
    string Modelo,
    int Anio,
    int Visitas);

public sealed record ModeloFrecuenteNegocioDto(string Marca, string Modelo, int Visitas);

public sealed record CobroNegocioDto(DateOnly Desde, DateOnly Hasta, decimal Monto, int Abonos);

public sealed record EstadoNegocioDto(
    DateOnly Desde,
    DateOnly Hasta,
    DateTime FechaConsultaUtc,
    decimal CobrosPeriodo,
    int AbonosPeriodo,
    decimal ImporteTrabajosPeriodo,
    int OrdenesPeriodo,
    decimal SaldoPendienteActual,
    decimal SaldoFavorActual,
    int ClientesPendientes,
    int ClientesLiquidados,
    int ClientesConSaldoFavor,
    IReadOnlyList<CobroNegocioDto> EvolucionCobros,
    IReadOnlyList<CuentaNegocioDto> Cuentas,
    IReadOnlyList<ClientePrincipalNegocioDto> ClientesPrincipales,
    IReadOnlyList<VehiculoFrecuenteNegocioDto> VehiculosFrecuentes,
    IReadOnlyList<ModeloFrecuenteNegocioDto> ModelosFrecuentes);
