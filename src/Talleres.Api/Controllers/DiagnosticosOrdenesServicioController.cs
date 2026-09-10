using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio/{ordenServicioId:long}/diagnostico")]
public sealed class DiagnosticosOrdenesServicioController(
    IDiagnosticoOrdenServicioServicio diagnosticoServicio) : ControllerBase
{
    private const long LongitudMaximaFotografia = 10 * 1024 * 1024;

    [HttpGet]
    public async Task<ActionResult<DiagnosticoOrdenServicioDto>> Obtener(long ordenServicioId, CancellationToken cancellationToken) =>
        Ok(await diagnosticoServicio.ObtenerAsync(ordenServicioId, cancellationToken));

    [HttpPut]
    public async Task<ActionResult<DiagnosticoOrdenServicioDto>> Guardar(long ordenServicioId, GuardarDiagnosticoOrdenServicioSolicitud solicitud, CancellationToken cancellationToken) =>
        Ok(await diagnosticoServicio.GuardarAsync(ordenServicioId, solicitud, cancellationToken));

    [HttpPost("evidencias")]
    [RequestFormLimits(MultipartBodyLengthLimit = 83_886_080)]
    [RequestSizeLimit(83_886_080)]
    public async Task<ActionResult<DiagnosticoOrdenServicioDto>> RegistrarEvidencias(
        long ordenServicioId,
        [FromForm] List<IFormFile> fotografias,
        CancellationToken cancellationToken)
    {
        if (fotografias.Count is 0 or > 8)
        {
            ModelState.AddModelError(nameof(fotografias), "Seleccione entre 1 y 8 fotografías por carga.");
            return ValidationProblem(ModelState);
        }

        var solicitudes = new List<RegistrarEvidenciaDiagnosticoSolicitud>(fotografias.Count);
        foreach (var fotografia in fotografias)
        {
            var error = await ValidarFotografiaAsync(fotografia, cancellationToken);
            if (error is not null) ModelState.AddModelError(nameof(fotografias), error);
            else solicitudes.Add(new(Path.GetFileName(fotografia.FileName), fotografia.ContentType, fotografia.Length, fotografia.OpenReadStream()));
        }
        if (!ModelState.IsValid)
        {
            foreach (var solicitud in solicitudes) await solicitud.Contenido.DisposeAsync();
            return ValidationProblem(ModelState);
        }
        try
        {
            return Ok(await diagnosticoServicio.RegistrarEvidenciasAsync(ordenServicioId, solicitudes, cancellationToken));
        }
        finally
        {
            foreach (var solicitud in solicitudes) await solicitud.Contenido.DisposeAsync();
        }
    }

    [HttpGet("evidencias/{evidenciaId:long}/contenido")]
    public async Task<IActionResult> ObtenerEvidencia(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken)
    {
        var direccion = await diagnosticoServicio.CrearDireccionEvidenciaAsync(ordenServicioId, evidenciaId, cancellationToken);
        return Redirect(direccion.AbsoluteUri);
    }

    [HttpDelete("evidencias/{evidenciaId:long}")]
    public async Task<IActionResult> EliminarEvidencia(long ordenServicioId, long evidenciaId, CancellationToken cancellationToken)
    {
        await diagnosticoServicio.EliminarEvidenciaAsync(ordenServicioId, evidenciaId, cancellationToken);
        return NoContent();
    }

    private static async Task<string?> ValidarFotografiaAsync(IFormFile fotografia, CancellationToken cancellationToken)
    {
        if (fotografia.Length == 0) return $"'{fotografia.FileName}' está vacía.";
        if (fotografia.Length > LongitudMaximaFotografia) return $"'{fotografia.FileName}' supera el máximo de 10 MB.";
        await using var contenido = fotografia.OpenReadStream();
        var cabecera = new byte[12];
        var leidos = await contenido.ReadAsync(cabecera, cancellationToken);
        var jpeg = fotografia.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase) && leidos >= 3 && cabecera.AsSpan(0, 3).SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF });
        var png = fotografia.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase) && leidos >= 8 && cabecera.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var webp = fotografia.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase) && leidos >= 12 && cabecera.AsSpan(0, 4).SequenceEqual("RIFF"u8) && cabecera.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        return jpeg || png || webp ? null : $"'{fotografia.FileName}' debe ser una imagen JPEG, PNG o WebP válida.";
    }
}

[ApiController]
[AllowAnonymous]
[Route("api/publico/ordenes-servicio/{token}")]
public sealed class OrdenesServicioPublicasController(
    IDiagnosticoOrdenServicioServicio diagnosticoServicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrdenServicioPublicaDto>> Obtener(string token, CancellationToken cancellationToken) =>
        Ok(await diagnosticoServicio.ObtenerPublicaAsync(token, cancellationToken));

    [HttpPost("autorizar")]
    public async Task<ActionResult<OrdenServicioPublicaDto>> Autorizar(string token, CancellationToken cancellationToken) =>
        Ok(await diagnosticoServicio.AutorizarPublicaAsync(token, cancellationToken));

    [HttpGet("evidencias-diagnostico/{evidenciaId:long}/contenido")]
    public async Task<IActionResult> ObtenerEvidencia(string token, long evidenciaId, CancellationToken cancellationToken)
    {
        var direccion = await diagnosticoServicio.CrearDireccionEvidenciaPublicaAsync(token, evidenciaId, cancellationToken);
        return Redirect(direccion.AbsoluteUri);
    }

    [HttpGet("evidencias-inspeccion/{evidenciaId:long}/contenido")]
    public async Task<IActionResult> ObtenerEvidenciaInspeccion(string token, long evidenciaId, CancellationToken cancellationToken)
    {
        var direccion = await diagnosticoServicio.CrearDireccionEvidenciaInspeccionPublicaAsync(token, evidenciaId, cancellationToken);
        return Redirect(direccion.AbsoluteUri);
    }
}
