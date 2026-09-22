using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class OrdenServicioServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IOrdenServicioServicio
{
    private static readonly TimeSpan PermanenciaListaParaEntregaEnInicio = TimeSpan.FromHours(36);

    public async Task<OrdenServicioDto> CrearAsync(
        CrearOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes
                          .AsNoTracking()
                          .SingleOrDefaultAsync(
                              item => item.Id == solicitud.ClienteId && item.Activo,
                              cancellationToken)
                      ?? throw new RecursoNoEncontradoException(
                          "El cliente indicado no existe o está inactivo.");

        var vehiculo = await dbContext.Vehiculos
                           .AsNoTracking()
                           .SingleOrDefaultAsync(
                               item => item.Id == solicitud.VehiculoId && item.Activo,
                               cancellationToken)
                       ?? throw new RecursoNoEncontradoException(
                           "El vehículo indicado no existe o está inactivo.");

        if (vehiculo.ClienteId != cliente.Id)
        {
            throw new ReglaNegocioException(
                "El vehículo seleccionado no pertenece al cliente indicado.");
        }

        var fechaActual = DateTime.UtcNow;
        var orden = new OrdenServicio
        {
            EmpresaId = empresaId,
            Numero = GenerarNumero(fechaActual),
            ClienteId = cliente.Id,
            VehiculoId = vehiculo.Id,
            Estado = EstadoOrdenServicio.Recepcion,
            FechaIngreso = fechaActual,
            Observaciones = LimpiarOpcional(solicitud.Observaciones)
        };

        orden.Historial.Add(new HistorialOrdenServicio
        {
            EmpresaId = empresaId,
            EstadoAnterior = null,
            EstadoNuevo = EstadoOrdenServicio.Recepcion,
            Descripcion = "Orden creada y pendiente de recepción del vehículo.",
            Fecha = fechaActual
        });

        dbContext.OrdenesServicio.Add(orden);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ConvertirDto(orden, cliente.Nombre, vehiculo.Placa, false, fechaActual);
    }

    public async Task<OrdenServicioDto> ObtenerPorIdAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await ProyectarDto(ConsultarOrdenes())
                   .SingleOrDefaultAsync(orden => orden.Id == ordenServicioId, cancellationToken)
               ?? throw new RecursoNoEncontradoException(
                   "La orden de servicio solicitada no existe.");
    }

    public async Task<IReadOnlyCollection<OrdenServicioDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var consultaOrdenada = ConsultarOrdenes()
            .OrderByDescending(orden => orden.FechaIngreso);

        return await ProyectarDto(consultaOrdenada)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<OrdenServicioDto> CambiarEstadoAsync(
        long ordenServicioId,
        CambiarEstadoOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio
                        .Include(item => item.Cliente)
                        .Include(item => item.Vehiculo)
                        .Include(item => item.Recepcion)
                        .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
                    ?? throw new RecursoNoEncontradoException(
                        "La orden de servicio solicitada no existe.");

        if (!PuedeTransicionar(orden.Estado, solicitud.Estado))
        {
            throw new ReglaNegocioException(
                $"No se permite cambiar la orden de {orden.Estado} a {solicitud.Estado}.");
        }

        if (orden.Estado == EstadoOrdenServicio.Diagnostico &&
            solicitud.Estado == EstadoOrdenServicio.PendienteAprobacion &&
            string.IsNullOrWhiteSpace(orden.Diagnostico))
        {
            throw new ReglaNegocioException(
                "Debe guardar el diagnóstico antes de enviarlo para autorización.");
        }

        if (orden.Estado == EstadoOrdenServicio.PendienteAprobacion &&
            solicitud.Estado == EstadoOrdenServicio.Reparacion &&
            orden.FechaAutorizacionClienteUtc is null)
        {
            throw new ReglaNegocioException(
                "El cliente todavía no ha autorizado continuar con la reparación.");
        }

        var estadoAnterior = orden.Estado;
        var fechaCambioEstado = DateTime.UtcNow;
        orden.Estado = solicitud.Estado;
        orden.Historial.Add(new HistorialOrdenServicio
        {
            EmpresaId = empresaId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = solicitud.Estado,
            Descripcion = solicitud.Descripcion.Trim(),
            Fecha = fechaCambioEstado
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return ConvertirDto(
            orden,
            orden.Cliente.Nombre,
            orden.Vehiculo.Placa,
            orden.Recepcion is not null,
            fechaCambioEstado);
    }

    private IQueryable<OrdenServicio> ConsultarOrdenes() =>
        dbContext.OrdenesServicio.AsNoTracking();

    private static IQueryable<OrdenServicioDto> ProyectarDto(
        IQueryable<OrdenServicio> consulta)
    {
        var limiteListaParaEntrega = DateTime.UtcNow.Subtract(PermanenciaListaParaEntregaEnInicio);

        return consulta.Select(orden => new OrdenServicioDto(
            orden.Id,
            orden.Numero,
            orden.ClienteId,
            orden.Cliente.Nombre,
            orden.VehiculoId,
            orden.Vehiculo.Placa,
            orden.Estado,
            orden.FechaIngreso,
            orden.Historial
                .Where(historial => historial.EstadoNuevo == orden.Estado)
                .OrderByDescending(historial => historial.Fecha)
                .ThenByDescending(historial => historial.Id)
                .Select(historial => (DateTime?)historial.Fecha)
                .FirstOrDefault() ?? orden.FechaIngreso,
            orden.Observaciones,
            orden.Recepcion != null,
            orden.Estado != EstadoOrdenServicio.ListaParaEntrega ||
            (orden.Historial
                .Where(historial => historial.EstadoNuevo == orden.Estado)
                .OrderByDescending(historial => historial.Fecha)
                .ThenByDescending(historial => historial.Id)
                .Select(historial => (DateTime?)historial.Fecha)
                .FirstOrDefault() ?? orden.FechaIngreso) > limiteListaParaEntrega));
    }

    private static OrdenServicioDto ConvertirDto(
        OrdenServicio orden,
        string nombreCliente,
        string placaVehiculo,
        bool tieneRecepcion,
        DateTime fechaUltimoCambioEstado) => new(
            orden.Id,
            orden.Numero,
            orden.ClienteId,
            nombreCliente,
            orden.VehiculoId,
            placaVehiculo,
            orden.Estado,
            orden.FechaIngreso,
            fechaUltimoCambioEstado,
            orden.Observaciones,
            tieneRecepcion,
            orden.Estado != EstadoOrdenServicio.ListaParaEntrega ||
            fechaUltimoCambioEstado > DateTime.UtcNow.Subtract(PermanenciaListaParaEntregaEnInicio));

    private static string GenerarNumero(DateTime fecha) =>
        $"OS-{fecha:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..25].ToUpperInvariant();

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static bool PuedeTransicionar(
        EstadoOrdenServicio estadoActual,
        EstadoOrdenServicio nuevoEstado)
    {
        if (nuevoEstado == EstadoOrdenServicio.Cancelada)
        {
            return estadoActual is not EstadoOrdenServicio.Entregada
                and not EstadoOrdenServicio.Cerrada
                and not EstadoOrdenServicio.Cancelada;
        }

        return (estadoActual, nuevoEstado) switch
        {
            (EstadoOrdenServicio.Recepcion, EstadoOrdenServicio.Diagnostico) => true,
            (EstadoOrdenServicio.Diagnostico, EstadoOrdenServicio.PendienteAprobacion) => true,
            (EstadoOrdenServicio.Diagnostico, EstadoOrdenServicio.Cotizacion) => true,
            (EstadoOrdenServicio.Cotizacion, EstadoOrdenServicio.PendienteAprobacion) => true,
            (EstadoOrdenServicio.PendienteAprobacion, EstadoOrdenServicio.Reparacion) => true,
            (EstadoOrdenServicio.PendienteAprobacion, EstadoOrdenServicio.PreparacionReparacion) => true,
            (EstadoOrdenServicio.PreparacionReparacion, EstadoOrdenServicio.Reparacion) => true,
            (EstadoOrdenServicio.Reparacion, EstadoOrdenServicio.ListaParaEntrega) => true,
            (EstadoOrdenServicio.Reparacion, EstadoOrdenServicio.ControlCalidad) => true,
            (EstadoOrdenServicio.ControlCalidad, EstadoOrdenServicio.Reparacion) => true,
            (EstadoOrdenServicio.ControlCalidad, EstadoOrdenServicio.ListaParaEntrega) => true,
            (EstadoOrdenServicio.ListaParaEntrega, EstadoOrdenServicio.Entregada) => true,
            (EstadoOrdenServicio.Entregada, EstadoOrdenServicio.Cerrada) => true,
            _ => false
        };
    }
}
