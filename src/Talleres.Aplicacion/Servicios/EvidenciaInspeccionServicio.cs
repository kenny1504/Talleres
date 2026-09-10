using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class EvidenciaInspeccionServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa,
    IAlmacenamientoEvidencias almacenamientoEvidencias) : IEvidenciaInspeccionServicio
{
    private const int CantidadMaximaEvidencias = 12;

    public async Task<IReadOnlyCollection<EvidenciaInspeccionDto>> RegistrarAsync(
        long ordenServicioId,
        IReadOnlyCollection<RegistrarEvidenciaInspeccionSolicitud> solicitudes,
        CancellationToken cancellationToken = default)
    {
        if (solicitudes.Count == 0)
        {
            throw new ReglaNegocioException("Debe seleccionar al menos una fotografía.");
        }

        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var recepcion = await dbContext.RecepcionesVehiculo
                            .Include(item => item.Evidencias)
                            .SingleOrDefaultAsync(
                                item => item.OrdenServicioId == ordenServicioId,
                                cancellationToken)
                        ?? throw new RecursoNoEncontradoException(
                            "La orden no tiene una recepción registrada.");

        if (recepcion.Evidencias.Count + solicitudes.Count > CantidadMaximaEvidencias)
        {
            throw new ReglaNegocioException(
                $"Una inspección puede conservar como máximo {CantidadMaximaEvidencias} fotografías.");
        }

        var evidenciasCreadas = new List<EvidenciaInspeccion>(solicitudes.Count);
        var clavesGuardadas = new List<string>(solicitudes.Count);
        try
        {
            foreach (var solicitud in solicitudes)
            {
                var clave = await almacenamientoEvidencias.GuardarAsync(
                    empresaId,
                    recepcion.Id,
                    solicitud.NombreArchivo,
                    solicitud.TipoContenido,
                    solicitud.Contenido,
                    cancellationToken);
                clavesGuardadas.Add(clave);

                var evidencia = new EvidenciaInspeccion
                {
                    EmpresaId = empresaId,
                    RecepcionVehiculoId = recepcion.Id,
                    ClaveObjeto = clave,
                    NombreArchivo = solicitud.NombreArchivo,
                    TipoContenido = solicitud.TipoContenido,
                    Longitud = solicitud.Longitud,
                    FechaCargaUtc = DateTime.UtcNow
                };
                recepcion.Evidencias.Add(evidencia);
                evidenciasCreadas.Add(evidencia);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var clave in clavesGuardadas)
            {
                try
                {
                    await almacenamientoEvidencias.EliminarAsync(clave, CancellationToken.None);
                }
                catch
                {
                    // La falla original es más importante; S3 puede limpiar este objeto por ciclo de vida.
                }
            }

            throw;
        }

        return evidenciasCreadas.Select(ConvertirDto).ToArray();
    }

    public async Task<Uri> CrearDireccionLecturaAsync(
        long ordenServicioId,
        long evidenciaId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var evidencia = await dbContext.EvidenciasInspeccion
                            .AsNoTracking()
                            .Where(item => item.Id == evidenciaId)
                            .Where(item => item.RecepcionVehiculo.OrdenServicioId == ordenServicioId)
                            .Select(item => new { item.ClaveObjeto, item.NombreArchivo })
                            .SingleOrDefaultAsync(cancellationToken)
                        ?? throw new RecursoNoEncontradoException(
                            "La evidencia solicitada no existe.");

        return await almacenamientoEvidencias.CrearDireccionLecturaAsync(
            evidencia.ClaveObjeto,
            evidencia.NombreArchivo,
            cancellationToken);
    }

    public async Task EliminarAsync(
        long ordenServicioId,
        long evidenciaId,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var evidencia = await dbContext.EvidenciasInspeccion
                            .Where(item => item.Id == evidenciaId)
                            .Where(item => item.EmpresaId == empresaId)
                            .Where(item => item.RecepcionVehiculo.OrdenServicioId == ordenServicioId)
                            .SingleOrDefaultAsync(cancellationToken)
                        ?? throw new RecursoNoEncontradoException(
                            "La evidencia que intenta eliminar no existe en esta orden.");

        await almacenamientoEvidencias.EliminarAsync(
            evidencia.ClaveObjeto,
            cancellationToken);
        dbContext.EvidenciasInspeccion.Remove(evidencia);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static EvidenciaInspeccionDto ConvertirDto(EvidenciaInspeccion evidencia) => new(
        evidencia.Id,
        evidencia.NombreArchivo,
        evidencia.TipoContenido,
        evidencia.Longitud,
        evidencia.FechaCargaUtc);
}
