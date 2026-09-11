using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Enumeraciones;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class DiagnosticoOrdenServicioServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa,
    IAlmacenamientoEvidencias almacenamientoEvidencias,
    IIdentidadSmartNova identidadSmartNova) : IDiagnosticoOrdenServicioServicio
{
    private const int CantidadMaximaEvidencias = 8;

    public async Task<DiagnosticoOrdenServicioDto> ObtenerAsync(long ordenServicioId, CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio.AsNoTracking()
            .Include(item => item.EvidenciasDiagnostico)
            .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La orden de servicio solicitada no existe.");
        return ConvertirDiagnostico(orden);
    }

    public async Task<DiagnosticoOrdenServicioDto> GuardarAsync(long ordenServicioId, GuardarDiagnosticoOrdenServicioSolicitud solicitud, CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio
            .Include(item => item.EvidenciasDiagnostico)
            .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La orden de servicio solicitada no existe.");

        ValidarDiagnosticoEditable(orden);

        orden.Diagnostico = solicitud.Diagnostico.Trim();
        orden.FechaDiagnosticoUtc = DateTime.UtcNow;
        orden.TokenPublico ??= CrearTokenPublico();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirDiagnostico(orden);
    }

    public async Task<DiagnosticoOrdenServicioDto> RegistrarEvidenciasAsync(long ordenServicioId, IReadOnlyCollection<RegistrarEvidenciaDiagnosticoSolicitud> solicitudes, CancellationToken cancellationToken = default)
    {
        if (solicitudes.Count == 0) throw new ReglaNegocioException("Debe seleccionar al menos una fotografía.");
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio.Include(item => item.EvidenciasDiagnostico)
            .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La orden de servicio solicitada no existe.");
        if (orden.EvidenciasDiagnostico.Count + solicitudes.Count > CantidadMaximaEvidencias)
            throw new ReglaNegocioException($"El diagnóstico puede conservar como máximo {CantidadMaximaEvidencias} fotografías.");
        ValidarDiagnosticoEditable(orden);

        var claves = new List<string>();
        try
        {
            foreach (var solicitud in solicitudes)
            {
                var clave = await almacenamientoEvidencias.GuardarAsync(empresaId, orden.Id, solicitud.NombreArchivo, solicitud.TipoContenido, solicitud.Contenido, cancellationToken);
                claves.Add(clave);
                orden.EvidenciasDiagnostico.Add(new EvidenciaDiagnostico
                {
                    EmpresaId = empresaId,
                    ClaveObjeto = clave,
                    NombreArchivo = solicitud.NombreArchivo,
                    TipoContenido = solicitud.TipoContenido,
                    Longitud = solicitud.Longitud,
                    FechaCargaUtc = DateTime.UtcNow
                });
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var clave in claves)
            {
                try { await almacenamientoEvidencias.EliminarAsync(clave, CancellationToken.None); }
                catch { /* La limpieza posterior de S3 atenderá un objeto huérfano. */ }
            }
            throw;
        }
        return ConvertirDiagnostico(orden);
    }

    public async Task EliminarEvidenciaAsync(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var orden = await dbContext.OrdenesServicio
            .SingleOrDefaultAsync(item => item.Id == ordenServicioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La orden de servicio solicitada no existe.");
        ValidarDiagnosticoEditable(orden);
        var evidencia = await dbContext.EvidenciasDiagnostico
            .Where(item => item.Id == evidenciaId && item.OrdenServicioId == ordenServicioId)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("La evidencia del diagnóstico no existe.");
        await almacenamientoEvidencias.EliminarAsync(evidencia.ClaveObjeto, cancellationToken);
        dbContext.EvidenciasDiagnostico.Remove(evidencia);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Uri> CrearDireccionEvidenciaAsync(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var evidencia = await dbContext.EvidenciasDiagnostico.AsNoTracking()
            .Where(item => item.Id == evidenciaId && item.OrdenServicioId == ordenServicioId)
            .Select(item => new { item.ClaveObjeto, item.NombreArchivo })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("La evidencia del diagnóstico no existe.");
        return await almacenamientoEvidencias.CrearDireccionLecturaAsync(evidencia.ClaveObjeto, evidencia.NombreArchivo, cancellationToken);
    }

    public Task<OrdenServicioPublicaDto> ObtenerPublicaAsync(string token, CancellationToken cancellationToken = default) =>
        ProyectarPublicaAsync(NormalizarToken(token), cancellationToken);

    public async Task<OrdenServicioPublicaDto> AutorizarPublicaAsync(string token, CancellationToken cancellationToken = default)
    {
        token = NormalizarToken(token);
        var orden = await dbContext.OrdenesServicio.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.TokenPublico == token)
            .Select(item => new { item.Id, item.Estado, item.FechaAutorizacionClienteUtc })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("El enlace público no existe o dejó de estar disponible.");
        if (orden.Estado != EstadoOrdenServicio.PendienteAprobacion)
            throw new ReglaNegocioException("La orden no se encuentra pendiente de autorización.");

        if (orden.FechaAutorizacionClienteUtc is null)
        {
            await dbContext.OrdenesServicio.IgnoreQueryFilters()
                .Where(item => item.Id == orden.Id && item.TokenPublico == token && item.FechaAutorizacionClienteUtc == null)
                .ExecuteUpdateAsync(actualizacion => actualizacion
                    .SetProperty(item => item.FechaAutorizacionClienteUtc, DateTime.UtcNow), cancellationToken);
        }
        return await ProyectarPublicaAsync(token, cancellationToken);
    }

    public async Task<Uri> CrearDireccionEvidenciaPublicaAsync(string token, long evidenciaId, CancellationToken cancellationToken = default)
    {
        token = NormalizarToken(token);
        var evidencia = await dbContext.EvidenciasDiagnostico.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.Id == evidenciaId && item.OrdenServicio.TokenPublico == token)
            .Select(item => new { item.ClaveObjeto, item.NombreArchivo })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("La evidencia pública solicitada no existe.");
        return await almacenamientoEvidencias.CrearDireccionLecturaAsync(evidencia.ClaveObjeto, evidencia.NombreArchivo, cancellationToken);
    }

    public async Task<Uri> CrearDireccionEvidenciaInspeccionPublicaAsync(string token, long evidenciaId, CancellationToken cancellationToken = default)
    {
        token = NormalizarToken(token);
        var evidencia = await dbContext.EvidenciasInspeccion.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.Id == evidenciaId && item.RecepcionVehiculo.OrdenServicio.TokenPublico == token)
            .Select(item => new { item.ClaveObjeto, item.NombreArchivo })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("La fotografía pública de inspección solicitada no existe.");
        return await almacenamientoEvidencias.CrearDireccionLecturaAsync(evidencia.ClaveObjeto, evidencia.NombreArchivo, cancellationToken);
    }

    private async Task<OrdenServicioPublicaDto> ProyectarPublicaAsync(string token, CancellationToken cancellationToken)
    {
        var datos = await dbContext.OrdenesServicio.IgnoreQueryFilters().AsNoTracking()
            .Where(orden => orden.TokenPublico == token && orden.Diagnostico != null)
            .Select(orden => new
            {
                orden.EmpresaId,
                orden.Numero, orden.Estado, orden.FechaIngreso, orden.Cliente.Nombre,
                orden.Vehiculo.Placa, Marca = orden.Vehiculo.Modelo.Marca.Nombre,
                Modelo = orden.Vehiculo.Modelo.Nombre, orden.Vehiculo.Anio, orden.Vehiculo.Color,
                orden.Vehiculo.NumeroVin, MotivoIngreso = orden.Observaciones, orden.Diagnostico,
                orden.FechaAutorizacionClienteUtc,
                Kilometraje = orden.Recepcion == null ? (int?)null : orden.Recepcion.Kilometraje,
                Combustible = orden.Recepcion == null ? (byte?)null : orden.Recepcion.PorcentajeCombustible,
                DescripcionEstado = orden.Recepcion == null ? null : orden.Recepcion.DescripcionEstado,
                DejaLlaves = orden.Recepcion != null && orden.Recepcion.DejaLlaves,
                DejaDocumentos = orden.Recepcion != null && orden.Recepcion.DejaDocumentos,
                DaniosInspeccion = orden.Recepcion == null
                    ? Array.Empty<DanioVehiculoDto>()
                    : orden.Recepcion.Danios.Select(danio => new DanioVehiculoDto(danio.Id, danio.Zona, danio.Tipo, danio.Severidad, danio.Observacion)).ToArray(),
                EvidenciasInspeccion = orden.Recepcion == null
                    ? Array.Empty<EvidenciaInspeccionDto>()
                    : orden.Recepcion.Evidencias.Select(evidencia => new EvidenciaInspeccionDto(evidencia.Id, evidencia.NombreArchivo, evidencia.TipoContenido, evidencia.Longitud, evidencia.FechaCargaUtc)).ToArray(),
                Evidencias = orden.EvidenciasDiagnostico.Select(e => new EvidenciaDiagnosticoDto(e.Id, e.NombreArchivo, e.TipoContenido, e.Longitud, e.FechaCargaUtc)).ToArray(),
                Detalles = orden.Detalles.Select(d => new DetalleOrdenServicioDto(d.Id, d.Tipo, d.ProductoInventarioId, d.BodegaInventarioId, d.CodigoProducto, d.Descripcion, d.UnidadMedida, d.Cantidad, d.PrecioUnitario, d.Cantidad * d.PrecioUnitario, d.ExistenciaDescontada, d.FechaCreacion)).ToArray()
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new RecursoNoEncontradoException("El enlace público no existe o dejó de estar disponible.");
        var tallerNova = (await identidadSmartNova.ObtenerTalleresAsync(
            [checked((int)datos.EmpresaId)], cancellationToken)).SingleOrDefault()
            ?? throw new RecursoNoEncontradoException("No fue posible encontrar la información pública del taller.");
        var taller = new TallerPublicoDto(
            tallerNova.NombreComercial ?? tallerNova.NombreLegal,
            FormatearDireccion(tallerNova.Direccion, tallerNova.Ciudad, tallerNova.Barrio, tallerNova.Calle),
            FormatearTelefono(tallerNova.PrefijoTelefono, tallerNova.Telefono),
            tallerNova.Logo);
        return new OrdenServicioPublicaDto(taller, datos.Numero, datos.Estado, datos.FechaIngreso, datos.Nombre, datos.Placa, datos.Marca, datos.Modelo, datos.Anio, datos.Color, datos.NumeroVin, datos.MotivoIngreso, datos.Diagnostico!, datos.FechaAutorizacionClienteUtc, datos.Kilometraje, datos.Combustible, datos.DescripcionEstado, datos.DejaLlaves, datos.DejaDocumentos, datos.DaniosInspeccion, datos.EvidenciasInspeccion, datos.Evidencias, datos.Detalles, datos.Detalles.Sum(item => item.Subtotal));
    }

    private static string? FormatearDireccion(params string?[] partes)
    {
        var direccion = string.Join(", ", partes.Where(parte => !string.IsNullOrWhiteSpace(parte)).Select(parte => parte!.Trim()));
        return string.IsNullOrWhiteSpace(direccion) ? null : direccion;
    }

    private static string? FormatearTelefono(int prefijo, string telefono)
    {
        telefono = telefono.Trim();
        if (string.IsNullOrWhiteSpace(telefono)) return null;
        return prefijo > 0 ? $"+{prefijo} {telefono}" : telefono;
    }

    private static DiagnosticoOrdenServicioDto ConvertirDiagnostico(OrdenServicio orden) => new(
        orden.Diagnostico, orden.FechaDiagnosticoUtc, orden.TokenPublico, orden.FechaAutorizacionClienteUtc,
        orden.EvidenciasDiagnostico.OrderBy(item => item.FechaCargaUtc).Select(item => new EvidenciaDiagnosticoDto(item.Id, item.NombreArchivo, item.TipoContenido, item.Longitud, item.FechaCargaUtc)).ToArray());

    private static void ValidarDiagnosticoEditable(OrdenServicio orden)
    {
        if (orden.Estado is not EstadoOrdenServicio.Diagnostico
            and not EstadoOrdenServicio.PendienteAprobacion
            and not EstadoOrdenServicio.PreparacionReparacion
            and not EstadoOrdenServicio.Reparacion
            and not EstadoOrdenServicio.ListaParaEntrega)
        {
            throw new ReglaNegocioException(
                "El diagnóstico solo puede modificarse durante el trabajo del taller y hasta que la orden sea entregada.");
        }
    }

    private static string CrearTokenPublico() => Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();

    private static string NormalizarToken(string token)
    {
        token = token.Trim().ToLowerInvariant();
        if (token.Length != 48 || token.Any(caracter => !Uri.IsHexDigit(caracter)))
            throw new RecursoNoEncontradoException("El enlace público no es válido.");
        return token;
    }
}
