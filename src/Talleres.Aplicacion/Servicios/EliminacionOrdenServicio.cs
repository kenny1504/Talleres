using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class EliminacionOrdenServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa,
    IInventarioSmartNova inventario,
    IAlmacenamientoEvidencias almacenamiento,
    ILogger<EliminacionOrdenServicio> logger) : IEliminacionOrdenServicio
{
    public async Task EliminarAsync(
        long ordenServicioId,
        int empresaNovaId,
        string usuarioId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        if (empresaId != empresaNovaId || string.IsNullOrWhiteSpace(usuarioId))
        {
            throw new AccesoDenegadoException("La sesión no contiene un usuario y taller válidos para eliminar la orden.");
        }

        var eliminacionIntentada = false;
        string[] clavesParaLimpiar = [];
        // Protege el estado y los consumos frente a cambios simultáneos durante las devoluciones.
        var clavesEvidencias = await dbContext.EjecutarConEstrategiaAsync(async () =>
        {
            await using var transaccion = await dbContext.IniciarTransaccionAsync(
                cancellationToken, IsolationLevel.Serializable);
            var orden = await dbContext.OrdenesServicio.SingleOrDefaultAsync(
                item => item.Id == ordenServicioId && item.EmpresaId == empresaId, cancellationToken);
            if (orden is null)
            {
                // La confirmación pudo completarse aunque se perdiera su respuesta.
                if (eliminacionIntentada) return clavesParaLimpiar;
                throw new RecursoNoEncontradoException("La orden de servicio solicitada no existe.");
            }

            var alcanzoEntrega = orden.Estado is EstadoOrdenServicio.ListaParaEntrega
                or EstadoOrdenServicio.Entregada or EstadoOrdenServicio.Cerrada;
            if (alcanzoEntrega || await dbContext.HistorialOrdenesServicio.AsNoTracking().AnyAsync(
                    item => item.EmpresaId == empresaId && item.OrdenServicioId == ordenServicioId &&
                            (item.EstadoNuevo == EstadoOrdenServicio.ListaParaEntrega ||
                             item.EstadoNuevo == EstadoOrdenServicio.Entregada ||
                             item.EstadoNuevo == EstadoOrdenServicio.Cerrada ||
                             item.EstadoAnterior == EstadoOrdenServicio.ListaParaEntrega ||
                             item.EstadoAnterior == EstadoOrdenServicio.Entregada ||
                             item.EstadoAnterior == EstadoOrdenServicio.Cerrada), cancellationToken))
            {
                throw new ReglaNegocioException(
                    "No se puede eliminar una orden que ya llegó a lista para entregar, entregada o cerrada.");
            }

            var consumos = await dbContext.DetallesOrdenesServicio
                .Where(item => item.EmpresaId == empresaId && item.OrdenServicioId == ordenServicioId)
                .ToListAsync(cancellationToken);
            var descontados = consumos.Where(item => item.ExistenciaDescontada).ToArray();
            if (descontados.Any(item => item.SalidaInventarioId is null or <= 0))
            {
                throw new ReglaNegocioException(
                    "La orden tiene productos descontados sin una salida de inventario válida para devolverlos.");
            }

            var claves = await dbContext.EvidenciasDiagnostico.AsNoTracking()
                .Where(item => item.EmpresaId == empresaId && item.OrdenServicioId == ordenServicioId)
                .Select(item => item.ClaveObjeto)
                .Concat(dbContext.EvidenciasInspeccion.AsNoTracking()
                    .Where(item => item.EmpresaId == empresaId && item.RecepcionVehiculo.OrdenServicioId == ordenServicioId)
                    .Select(item => item.ClaveObjeto))
                .Distinct().ToArrayAsync(cancellationToken);

            try
            {
                foreach (var grupo in descontados.GroupBy(item => item.SalidaInventarioId!.Value).OrderBy(grupo => grupo.Key))
                {
                    await inventario.AnularSalidaAsync(empresaNovaId, grupo.Key, usuarioId,
                        $"Eliminación de la orden {orden.Numero}.", cancellationToken);
                    foreach (var detalle in grupo)
                    {
                        detalle.ExistenciaDescontada = false;
                    }
                }
            }
            catch
            {
                // NOVA confirma cada devolución en su propia base. Conserva el avance si una posterior falla.
                await dbContext.SaveChangesAsync(CancellationToken.None);
                await transaccion.CommitAsync(CancellationToken.None);
                throw;
            }

            clavesParaLimpiar = claves;
            eliminacionIntentada = true;
            dbContext.OrdenesServicio.Remove(orden);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return claves;
        }, cancellationToken);

        foreach (var clave in clavesEvidencias)
        {
            try
            {
                await almacenamiento.EliminarAsync(clave, CancellationToken.None);
            }
            catch (Exception)
            {
                // La orden ya se eliminó. Los objetos pendientes deben limpiarse por el ciclo de vida del bucket.
                logger.LogWarning("No se pudo limpiar una fotografía de la orden eliminada {OrdenServicioId}.", ordenServicioId);
            }
        }
    }
}
