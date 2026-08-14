using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class RecepcionVehiculoServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IRecepcionVehiculoServicio
{
    public async Task<RecepcionVehiculoDto> RegistrarAsync(
        long ordenServicioId,
        RegistrarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio
                        .Include(item => item.Recepcion)
                        .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
                    ?? throw new RecursoNoEncontradoException(
                        "La orden de servicio solicitada no existe.");

        if (orden.Estado != EstadoOrdenServicio.Recepcion)
        {
            throw new ReglaNegocioException(
                "La orden debe encontrarse en recepción para registrar el vehículo.");
        }

        if (orden.Recepcion is not null)
        {
            throw new ReglaNegocioException(
                "La orden ya tiene una recepción de vehículo registrada.");
        }

        if (solicitud.Danios.Count > 30)
        {
            throw new ReglaNegocioException(
                "La inspección visual no puede contener más de 30 hallazgos.");
        }

        var fechaActual = DateTime.UtcNow;
        var recepcion = new RecepcionVehiculo
        {
            EmpresaId = empresaId,
            OrdenServicioId = orden.Id,
            Kilometraje = solicitud.Kilometraje,
            PorcentajeCombustible = solicitud.PorcentajeCombustible,
            DescripcionEstado = solicitud.DescripcionEstado.Trim(),
            DejaLlaves = solicitud.DejaLlaves,
            DejaDocumentos = solicitud.DejaDocumentos,
            FechaRecepcion = fechaActual
        };

        foreach (var danio in solicitud.Danios)
        {
            recepcion.Danios.Add(new DanioVehiculo
            {
                EmpresaId = empresaId,
                Zona = danio.Zona,
                Tipo = danio.Tipo,
                Severidad = danio.Severidad,
                Observacion = LimpiarOpcional(danio.Observacion)
            });
        }

        orden.Recepcion = recepcion;
        orden.Estado = EstadoOrdenServicio.Diagnostico;
        orden.Historial.Add(new HistorialOrdenServicio
        {
            EmpresaId = empresaId,
            EstadoAnterior = EstadoOrdenServicio.Recepcion,
            EstadoNuevo = EstadoOrdenServicio.Diagnostico,
            Descripcion = "Vehículo recibido; la orden pasa a diagnóstico.",
            Fecha = fechaActual
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirDto(recepcion);
    }

    public async Task<RecepcionVehiculoDto> ActualizarInspeccionAsync(
        long ordenServicioId,
        ActualizarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var recepcion = await dbContext.RecepcionesVehiculo
                            .Include(item => item.Danios)
                            .Include(item => item.OrdenServicio)
                            .SingleOrDefaultAsync(
                                item => item.OrdenServicioId == ordenServicioId,
                                cancellationToken)
                        ?? throw new RecursoNoEncontradoException(
                            "La orden no tiene una recepción registrada.");

        if (recepcion.OrdenServicio.Estado is EstadoOrdenServicio.Entregada
            or EstadoOrdenServicio.Cerrada
            or EstadoOrdenServicio.Cancelada)
        {
            throw new ReglaNegocioException(
                "No se puede modificar la inspección de una orden finalizada o cancelada.");
        }

        if (solicitud.Danios.Count > 30)
        {
            throw new ReglaNegocioException(
                "La inspección visual no puede contener más de 30 hallazgos.");
        }

        recepcion.Kilometraje = solicitud.Kilometraje;
        recepcion.PorcentajeCombustible = solicitud.PorcentajeCombustible;
        recepcion.DescripcionEstado = solicitud.DescripcionEstado.Trim();
        recepcion.DejaLlaves = solicitud.DejaLlaves;
        recepcion.DejaDocumentos = solicitud.DejaDocumentos;
        recepcion.Danios.Clear();

        foreach (var danio in solicitud.Danios)
        {
            recepcion.Danios.Add(new DanioVehiculo
            {
                EmpresaId = empresaId,
                Zona = danio.Zona,
                Tipo = danio.Tipo,
                Severidad = danio.Severidad,
                Observacion = LimpiarOpcional(danio.Observacion)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirDto(recepcion);
    }

    public async Task<RecepcionVehiculoDto> ObtenerPorOrdenAsync(
        long ordenServicioId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await dbContext.RecepcionesVehiculo
                   .AsNoTracking()
                   .Where(recepcion => recepcion.OrdenServicioId == ordenServicioId)
                   .Select(recepcion => new RecepcionVehiculoDto(
                       recepcion.Id,
                       recepcion.OrdenServicioId,
                       recepcion.Kilometraje,
                       recepcion.PorcentajeCombustible,
                       recepcion.DescripcionEstado,
                       recepcion.DejaLlaves,
                       recepcion.DejaDocumentos,
                       recepcion.FechaRecepcion,
                       recepcion.Danios
                           .OrderBy(danio => danio.Id)
                           .Select(danio => new DanioVehiculoDto(
                               danio.Id,
                               danio.Zona,
                               danio.Tipo,
                               danio.Severidad,
                               danio.Observacion))
                           .ToArray()))
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new RecursoNoEncontradoException(
                   "La orden no tiene una recepción registrada.");
    }

    private static RecepcionVehiculoDto ConvertirDto(RecepcionVehiculo recepcion) => new(
        recepcion.Id,
        recepcion.OrdenServicioId,
        recepcion.Kilometraje,
        recepcion.PorcentajeCombustible,
        recepcion.DescripcionEstado,
        recepcion.DejaLlaves,
        recepcion.DejaDocumentos,
        recepcion.FechaRecepcion,
        recepcion.Danios
            .Select(danio => new DanioVehiculoDto(
                danio.Id,
                danio.Zona,
                danio.Tipo,
                danio.Severidad,
                danio.Observacion))
            .ToArray());

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
