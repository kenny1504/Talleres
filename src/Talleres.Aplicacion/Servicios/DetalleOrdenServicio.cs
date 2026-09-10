using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Inventario;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class DetalleOrdenServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa,
    IInventarioSmartNova inventario) : IDetalleOrdenServicio
{
    public async Task<ResumenDetallesOrdenServicioDto> ObtenerAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default)
    {
        await ObtenerOrdenAsync(ordenServicioId, cancellationToken);
        return await ConstruirResumenAsync(ordenServicioId, cancellationToken);
    }

    public async Task<ResumenDetallesOrdenServicioDto> AgregarInventarioAsync(
        long ordenServicioId,
        AgregarDetalleInventarioSolicitud solicitud,
        int empresaNovaId,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await ObtenerOrdenAsync(ordenServicioId, cancellationToken);
        ValidarOrdenEnReparacion(orden);

        if (empresaId != empresaNovaId)
        {
            throw new ReglaNegocioException("La empresa activa no coincide con la empresa del inventario.");
        }

        var articulo = await inventario.ObtenerArticuloAsync(
                           empresaNovaId,
                           solicitud.BodegaId,
                           solicitud.ProductoId,
                           cancellationToken)
                       ?? throw new RecursoNoEncontradoException(
                           "El producto indicado no existe en el inventario de la empresa activa.");
        if (articulo.PrecioUnitario is null)
        {
            throw new ReglaNegocioException(
                "El producto no tiene un precio configurado en el inventario.");
        }

        var descontaraExistencia = articulo.Existencia >= solicitud.Cantidad;
        int? salidaInventarioId = null;
        if (descontaraExistencia)
        {
            try
            {
                salidaInventarioId = await inventario.RegistrarSalidaAsync(
                    new RegistrarSalidaInventarioSolicitud(
                        empresaNovaId,
                        solicitud.BodegaId,
                        articulo.ProductoId,
                        solicitud.Cantidad,
                        articulo.UnidadMedida,
                        usuarioId,
                        orden.Numero),
                    cancellationToken);
            }
            catch (ReglaNegocioException excepcion) when (
                excepcion.Message.Contains("stock insuficiente", StringComparison.OrdinalIgnoreCase) ||
                excepcion.Message.Contains("no existe stock", StringComparison.OrdinalIgnoreCase))
            {
                // La existencia pudo cambiar entre la consulta y la salida. El cargo se
                // conserva, pero se identifica expresamente como no descontado.
                descontaraExistencia = false;
            }
        }

        var detalle = new Talleres.Dominio.Entidades.DetalleOrdenServicio
        {
            EmpresaId = empresaId,
            OrdenServicioId = orden.Id,
            Tipo = TipoDetalleOrdenServicio.Inventario,
            ProductoInventarioId = articulo.ProductoId,
            BodegaInventarioId = solicitud.BodegaId,
            SalidaInventarioId = salidaInventarioId,
            CodigoProducto = articulo.Codigo,
            Descripcion = articulo.Nombre,
            UnidadMedida = articulo.UnidadMedida,
            Cantidad = solicitud.Cantidad,
            PrecioUnitario = articulo.PrecioUnitario.Value,
            ExistenciaDescontada = descontaraExistencia,
            FechaCreacion = DateTime.UtcNow
        };
        dbContext.DetallesOrdenesServicio.Add(detalle);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (salidaInventarioId.HasValue)
            {
                await inventario.AnularSalidaAsync(
                    empresaNovaId,
                    salidaInventarioId.Value,
                    usuarioId,
                    $"Compensación automática: no se pudo guardar el detalle de {orden.Numero}.",
                    CancellationToken.None);
            }

            throw;
        }

        return await ConstruirResumenAsync(ordenServicioId, cancellationToken);
    }

    public async Task<ResumenDetallesOrdenServicioDto> AgregarManualAsync(
        long ordenServicioId,
        AgregarDetalleManualSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await ObtenerOrdenAsync(ordenServicioId, cancellationToken);
        ValidarOrdenEnReparacion(orden);

        dbContext.DetallesOrdenesServicio.Add(new Talleres.Dominio.Entidades.DetalleOrdenServicio
        {
            EmpresaId = empresaId,
            OrdenServicioId = orden.Id,
            Tipo = TipoDetalleOrdenServicio.Manual,
            Descripcion = solicitud.Descripcion.Trim(),
            UnidadMedida = LimpiarOpcional(solicitud.UnidadMedida),
            Cantidad = solicitud.Cantidad,
            PrecioUnitario = solicitud.PrecioUnitario,
            ExistenciaDescontada = false,
            FechaCreacion = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ConstruirResumenAsync(ordenServicioId, cancellationToken);
    }

    public async Task<ResumenDetallesOrdenServicioDto> EliminarAsync(
        long ordenServicioId,
        long detalleId,
        CancellationToken cancellationToken = default)
    {
        var orden = await ObtenerOrdenAsync(ordenServicioId, cancellationToken);
        ValidarOrdenEnReparacion(orden);
        var detalle = await dbContext.DetallesOrdenesServicio.SingleOrDefaultAsync(
                          item => item.Id == detalleId && item.OrdenServicioId == ordenServicioId,
                          cancellationToken)
                      ?? throw new RecursoNoEncontradoException(
                          "El detalle solicitado no existe en esta orden.");
        if (detalle.ExistenciaDescontada)
        {
            throw new ReglaNegocioException(
                "El producto ya generó una salida de inventario y no puede eliminarse desde la orden.");
        }

        dbContext.DetallesOrdenesServicio.Remove(detalle);
        await dbContext.SaveChangesAsync(cancellationToken);
        return await ConstruirResumenAsync(ordenServicioId, cancellationToken);
    }

    private async Task<OrdenServicio> ObtenerOrdenAsync(
        long ordenServicioId,
        CancellationToken cancellationToken)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await dbContext.OrdenesServicio.SingleOrDefaultAsync(
                   item => item.Id == ordenServicioId,
                   cancellationToken)
               ?? throw new RecursoNoEncontradoException(
                   "La orden de servicio solicitada no existe.");
    }

    private async Task<ResumenDetallesOrdenServicioDto> ConstruirResumenAsync(
        long ordenServicioId,
        CancellationToken cancellationToken)
    {
        var detalles = await dbContext.DetallesOrdenesServicio
            .AsNoTracking()
            .Where(item => item.OrdenServicioId == ordenServicioId)
            .OrderBy(item => item.FechaCreacion)
            .Select(item => new DetalleOrdenServicioDto(
                item.Id,
                item.Tipo,
                item.ProductoInventarioId,
                item.BodegaInventarioId,
                item.CodigoProducto,
                item.Descripcion,
                item.UnidadMedida,
                item.Cantidad,
                item.PrecioUnitario,
                item.Cantidad * item.PrecioUnitario,
                item.ExistenciaDescontada,
                item.FechaCreacion))
            .ToArrayAsync(cancellationToken);
        return new ResumenDetallesOrdenServicioDto(
            detalles,
            detalles.Sum(item => item.Subtotal));
    }

    private static void ValidarOrdenEnReparacion(OrdenServicio orden)
    {
        if (orden.Estado != EstadoOrdenServicio.Reparacion)
        {
            throw new ReglaNegocioException(
                "Los productos y cargos solo pueden modificarse mientras la orden está en reparación.");
        }
    }

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
