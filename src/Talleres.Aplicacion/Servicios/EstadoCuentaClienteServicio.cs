using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class EstadoCuentaClienteServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IEstadoCuentaClienteServicio
{
    public async Task<EstadoCuentaClienteDto> ObtenerAsync(
        long clienteId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes.AsNoTracking()
            .Where(item => item.Id == clienteId && item.EmpresaId == empresaId)
            .Select(item => new { item.Id, item.Nombre })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("El cliente solicitado no existe.");

        var cargos = await dbContext.OrdenesServicio.AsNoTracking()
            .Where(orden => orden.EmpresaId == empresaId &&
                            orden.ClienteId == clienteId &&
                            orden.Estado != EstadoOrdenServicio.Cancelada)
            .OrderByDescending(orden => orden.FechaIngreso)
            .ThenByDescending(orden => orden.Id)
            .Select(orden => new
            {
                orden.Id,
                orden.Numero,
                orden.Estado,
                orden.FechaIngreso,
                Total = orden.Detalles.Sum(detalle =>
                    (decimal?)(detalle.Cantidad * detalle.PrecioUnitario)) ?? 0m
            })
            .ToArrayAsync(cancellationToken);
        var ordenes = cargos.Select(orden => new OrdenEstadoCuentaClienteDto(
            orden.Id,
            orden.Numero,
            orden.Estado,
            orden.FechaIngreso,
            decimal.Round(orden.Total, 2, MidpointRounding.AwayFromZero))).ToArray();

        var pagos = await dbContext.PagosClientes.AsNoTracking()
            .Where(pago => pago.EmpresaId == empresaId && pago.ClienteId == clienteId)
            .OrderByDescending(pago => pago.FechaRegistroUtc)
            .ThenByDescending(pago => pago.Id)
            .Select(pago => new PagoClienteDto(
                pago.Id,
                pago.Monto,
                pago.FechaRegistroUtc,
                pago.FormaPago,
                pago.Referencia,
                pago.FechaAnulacionUtc))
            .ToArrayAsync(cancellationToken);

        var totalOrdenes = ordenes.Sum(orden => orden.Total);
        var totalPagos = pagos.Where(pago => pago.FechaAnulacionUtc is null)
            .Sum(pago => pago.Monto);
        return new EstadoCuentaClienteDto(
            cliente.Id,
            cliente.Nombre,
            totalOrdenes,
            totalPagos,
            totalOrdenes - totalPagos,
            ordenes,
            pagos);
    }

    public async Task<EstadoCuentaClienteDto> RegistrarPagoAsync(
        long clienteId,
        RegistrarPagoClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var clienteExiste = await dbContext.Clientes.AsNoTracking()
            .AnyAsync(cliente => cliente.Id == clienteId && cliente.EmpresaId == empresaId,
                cancellationToken);
        if (!clienteExiste)
        {
            throw new RecursoNoEncontradoException("El cliente solicitado no existe.");
        }

        if (solicitud.Monto <= 0 ||
            solicitud.Monto != decimal.Round(solicitud.Monto, 2, MidpointRounding.AwayFromZero))
        {
            throw new ReglaNegocioException("El monto debe ser positivo y tener como máximo dos decimales.");
        }

        dbContext.PagosClientes.Add(new PagoCliente
        {
            EmpresaId = empresaId,
            ClienteId = clienteId,
            Monto = solicitud.Monto,
            FechaRegistroUtc = DateTime.UtcNow,
            FormaPago = LimpiarOpcional(solicitud.FormaPago),
            Referencia = LimpiarOpcional(solicitud.Referencia)
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerAsync(clienteId, cancellationToken);
    }

    public async Task<EstadoCuentaClienteDto> AnularPagoAsync(
        long clienteId,
        long pagoId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var pago = await dbContext.PagosClientes.SingleOrDefaultAsync(
            item => item.Id == pagoId &&
                    item.ClienteId == clienteId &&
                    item.EmpresaId == empresaId,
            cancellationToken)
            ?? throw new RecursoNoEncontradoException("El pago solicitado no existe para este cliente.");
        if (pago.FechaAnulacionUtc is not null)
        {
            throw new ReglaNegocioException("Este pago ya está anulado.");
        }

        pago.FechaAnulacionUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ObtenerAsync(clienteId, cancellationToken);
    }

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
