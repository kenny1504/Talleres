using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.EstadoNegocio;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class EstadoNegocioServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IEstadoNegocioServicio
{
    public async Task<EstadoNegocioDto> ObtenerAsync(
        ConsultarEstadoNegocioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        ValidarPeriodo(solicitud);
        var desde = solicitud.Desde!.Value;
        var hasta = solicitud.Hasta!.Value;
        var inicioUtc = DateTime.SpecifyKind(
            desde.ToDateTime(TimeOnly.MinValue).AddMinutes(solicitud.DesfaseHorarioMinutos), DateTimeKind.Utc);
        var finUtc = DateTime.SpecifyKind(
            hasta.AddDays(1).ToDateTime(TimeOnly.MinValue).AddMinutes(solicitud.DesfaseHorarioMinutos), DateTimeKind.Utc);

        var ordenes = await dbContext.OrdenesServicio.AsNoTracking()
            .Where(orden => orden.EmpresaId == empresaId && orden.Cliente.EmpresaId == empresaId &&
                orden.Vehiculo.EmpresaId == empresaId && orden.Vehiculo.Modelo.EmpresaId == empresaId &&
                orden.Vehiculo.Modelo.Marca.EmpresaId == empresaId &&
                orden.Estado != EstadoOrdenServicio.Cancelada &&
                orden.FechaIngreso >= inicioUtc && orden.FechaIngreso < finUtc)
            .Select(orden => new
            {
                orden.ClienteId,
                NombreCliente = orden.Cliente.Nombre,
                orden.VehiculoId,
                orden.Vehiculo.Placa,
                Marca = orden.Vehiculo.Modelo.Marca.Nombre,
                Modelo = orden.Vehiculo.Modelo.Nombre,
                orden.Vehiculo.Anio,
                Total = orden.Detalles.Where(detalle => detalle.EmpresaId == empresaId)
                    .Sum(detalle => (decimal?)(detalle.Cantidad * detalle.PrecioUnitario)) ?? 0m
            }).ToArrayAsync(cancellationToken);

        var cobrosDiarios = await dbContext.PagosClientes.AsNoTracking()
            .Where(pago => pago.EmpresaId == empresaId && pago.Cliente.EmpresaId == empresaId &&
                pago.FechaAnulacionUtc == null && pago.FechaRegistroUtc >= inicioUtc && pago.FechaRegistroUtc < finUtc)
            .GroupBy(pago => pago.FechaRegistroUtc.AddMinutes(-solicitud.DesfaseHorarioMinutos).Date)
            .Select(grupo => new { Fecha = grupo.Key, Monto = grupo.Sum(pago => pago.Monto), Abonos = grupo.Count() })
            .ToArrayAsync(cancellationToken);
        var cobrosPorDia = cobrosDiarios.ToDictionary(
            dia => DateOnly.FromDateTime(dia.Fecha),
            dia => new CobroNegocioDto(DateOnly.FromDateTime(dia.Fecha), DateOnly.FromDateTime(dia.Fecha), dia.Monto, dia.Abonos));
        var cuentas = await ObtenerCuentasAsync(empresaId, cancellationToken);

        return new EstadoNegocioDto(
            desde, hasta, DateTime.UtcNow,
            cobrosDiarios.Sum(dia => dia.Monto), cobrosDiarios.Sum(dia => dia.Abonos),
            ordenes.Sum(orden => Redondear(orden.Total)), ordenes.Length,
            cuentas.Where(cuenta => cuenta.Saldo > 0).Sum(cuenta => cuenta.Saldo),
            -cuentas.Where(cuenta => cuenta.Saldo < 0).Sum(cuenta => cuenta.Saldo),
            cuentas.Count(cuenta => cuenta.Situacion == SituacionCuentaNegocio.Pendiente),
            cuentas.Count(cuenta => cuenta.Situacion == SituacionCuentaNegocio.Liquidada),
            cuentas.Count(cuenta => cuenta.Situacion == SituacionCuentaNegocio.SaldoAFavor),
            AgruparCobros(desde, hasta, solicitud.Agrupacion, cobrosPorDia),
            cuentas,
            ordenes.GroupBy(orden => new { orden.ClienteId, orden.NombreCliente })
                .Select(grupo => new ClientePrincipalNegocioDto(grupo.Key.ClienteId, grupo.Key.NombreCliente,
                    grupo.Count(), grupo.Sum(orden => Redondear(orden.Total))))
                .Where(cliente => cliente.Importe > 0)
                .OrderByDescending(cliente => cliente.Importe).ThenBy(cliente => cliente.Nombre).Take(5).ToArray(),
            ordenes.GroupBy(orden => new { orden.VehiculoId, orden.Placa, orden.Marca, orden.Modelo, orden.Anio })
                .Select(grupo => new VehiculoFrecuenteNegocioDto(grupo.Key.VehiculoId, grupo.Key.Placa,
                    grupo.Key.Marca, grupo.Key.Modelo, grupo.Key.Anio, grupo.Count()))
                .OrderByDescending(vehiculo => vehiculo.Visitas).ThenBy(vehiculo => vehiculo.Placa).Take(5).ToArray(),
            ordenes.GroupBy(orden => new { orden.Marca, orden.Modelo })
                .Select(grupo => new ModeloFrecuenteNegocioDto(grupo.Key.Marca, grupo.Key.Modelo, grupo.Count()))
                .OrderByDescending(modelo => modelo.Visitas).ThenBy(modelo => modelo.Marca)
                .ThenBy(modelo => modelo.Modelo).Take(5).ToArray());
    }

    private async Task<CuentaNegocioDto[]> ObtenerCuentasAsync(long empresaId, CancellationToken cancellationToken)
    {
        // Los cargos son positivos. Floor evita diferencias entre el redondeo de SQL Server
        // y el de .NET al conservar dos decimales por orden, como el estado de cuenta individual.
        var cuentas = await dbContext.Clientes.AsNoTracking()
            .Where(cliente => cliente.EmpresaId == empresaId)
            .Select(cliente => new
            {
                cliente.Id,
                cliente.Nombre,
                Trabajos = cliente.OrdenesServicio
                    .Where(orden => orden.EmpresaId == empresaId && orden.Estado != EstadoOrdenServicio.Cancelada)
                    .Sum(orden => (decimal?)(Math.Floor(((orden.Detalles.Where(detalle => detalle.EmpresaId == empresaId)
                        .Sum(detalle => (decimal?)(detalle.Cantidad * detalle.PrecioUnitario)) ?? 0m) * 100m) + 0.5m) / 100m)) ?? 0m,
                Abonos = cliente.Pagos.Where(pago => pago.EmpresaId == empresaId && pago.FechaAnulacionUtc == null)
                    .Sum(pago => (decimal?)pago.Monto) ?? 0m
            })
            .Where(cuenta => cuenta.Trabajos > 0 || cuenta.Abonos > 0)
            .ToArrayAsync(cancellationToken);

        return cuentas.Select(cuenta => new CuentaNegocioDto(
                cuenta.Id, cuenta.Nombre, cuenta.Trabajos, cuenta.Abonos, cuenta.Trabajos - cuenta.Abonos,
                cuenta.Trabajos > cuenta.Abonos ? SituacionCuentaNegocio.Pendiente :
                cuenta.Trabajos < cuenta.Abonos ? SituacionCuentaNegocio.SaldoAFavor : SituacionCuentaNegocio.Liquidada))
            .OrderByDescending(cuenta => cuenta.Saldo).ThenBy(cuenta => cuenta.Nombre).ToArray();
    }

    private static CobroNegocioDto[] AgruparCobros(
        DateOnly desde, DateOnly hasta, AgrupacionCobrosNegocio agrupacion,
        IReadOnlyDictionary<DateOnly, CobroNegocioDto> cobrosPorDia)
    {
        var resultado = new List<CobroNegocioDto>();
        for (var inicio = desde; inicio <= hasta;)
        {
            var fin = agrupacion switch
            {
                AgrupacionCobrosNegocio.Semana => inicio.AddDays((7 - (int)inicio.DayOfWeek) % 7),
                AgrupacionCobrosNegocio.Mes => new DateOnly(inicio.Year, inicio.Month, DateTime.DaysInMonth(inicio.Year, inicio.Month)),
                _ => inicio
            };
            if (fin > hasta) fin = hasta;
            decimal monto = 0;
            var abonos = 0;
            for (var fecha = inicio; fecha <= fin; fecha = fecha.AddDays(1))
            {
                if (!cobrosPorDia.TryGetValue(fecha, out var cobro)) continue;
                monto += cobro.Monto;
                abonos += cobro.Abonos;
            }
            resultado.Add(new CobroNegocioDto(inicio, fin, monto, abonos));
            inicio = fin.AddDays(1);
        }
        return resultado.ToArray();
    }

    private static void ValidarPeriodo(ConsultarEstadoNegocioSolicitud solicitud)
    {
        if (solicitud.Desde is not { } desde || solicitud.Hasta is not { } hasta ||
            desde.Year < 2000 || hasta.Year > 9998 || desde > hasta || hasta.DayNumber - desde.DayNumber >= 366)
            throw new ReglaNegocioException("Selecciona un período válido de hasta 366 días, desde el año 2000.");
        if (solicitud.DesfaseHorarioMinutos is < -840 or > 840 || !Enum.IsDefined(solicitud.Agrupacion))
            throw new ReglaNegocioException("La zona horaria o la agrupación de cobros no es válida.");
    }

    private static decimal Redondear(decimal importe) => decimal.Round(importe, 2, MidpointRounding.AwayFromZero);
}
