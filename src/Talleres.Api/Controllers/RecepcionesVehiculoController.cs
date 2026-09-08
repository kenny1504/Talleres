using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio/{ordenServicioId:long}/recepcion")]
public sealed class RecepcionesVehiculoController(
    IRecepcionVehiculoServicio recepcionVehiculoServicio,
    IEvidenciaInspeccionServicio evidenciaInspeccionServicio) : ControllerBase
{
    private const long LongitudMaximaFotografia = 10 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, byte[]> FirmasPermitidas =
        new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = [0xFF, 0xD8, 0xFF],
            ["image/png"] = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
        };

    [HttpGet]
    [ProducesResponseType<RecepcionVehiculoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RecepcionVehiculoDto>> ObtenerPorOrden(
        long ordenServicioId,
        CancellationToken cancellationToken)
    {
        var recepcion = await recepcionVehiculoServicio.ObtenerPorOrdenAsync(
            ordenServicioId,
            cancellationToken);
        return Ok(recepcion);
    }

    [HttpPost]
    [ProducesResponseType<RecepcionVehiculoDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<RecepcionVehiculoDto>> Registrar(
        long ordenServicioId,
        RegistrarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var recepcion = await recepcionVehiculoServicio.RegistrarAsync(
            ordenServicioId,
            solicitud,
            cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorOrden),
            new { ordenServicioId },
            recepcion);
    }

    [HttpPut]
    [ProducesResponseType<RecepcionVehiculoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RecepcionVehiculoDto>> ActualizarInspeccion(
        long ordenServicioId,
        ActualizarRecepcionVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var recepcion = await recepcionVehiculoServicio.ActualizarInspeccionAsync(
            ordenServicioId,
            solicitud,
            cancellationToken);
        return Ok(recepcion);
    }

    [HttpPost("evidencias")]
    [RequestFormLimits(MultipartBodyLengthLimit = 125_829_120)]
    [RequestSizeLimit(125_829_120)]
    [ProducesResponseType<IReadOnlyCollection<EvidenciaInspeccionDto>>(StatusCodes.Status201Created)]
    public async Task<ActionResult<IReadOnlyCollection<EvidenciaInspeccionDto>>> RegistrarEvidencias(
        long ordenServicioId,
        [FromForm] List<IFormFile> fotografias,
        CancellationToken cancellationToken)
    {
        if (fotografias.Count is 0 or > 12)
        {
            ModelState.AddModelError(
                nameof(fotografias),
                "Seleccione entre 1 y 12 fotografías por carga.");
            return ValidationProblem(ModelState);
        }

        var solicitudes = new List<RegistrarEvidenciaInspeccionSolicitud>(fotografias.Count);
        foreach (var fotografia in fotografias)
        {
            var error = await ValidarFotografiaAsync(fotografia, cancellationToken);
            if (error is not null)
            {
                ModelState.AddModelError(nameof(fotografias), error);
                continue;
            }

            solicitudes.Add(new RegistrarEvidenciaInspeccionSolicitud(
                Path.GetFileName(fotografia.FileName),
                fotografia.ContentType,
                fotografia.Length,
                fotografia.OpenReadStream()));
        }

        if (!ModelState.IsValid)
        {
            foreach (var solicitud in solicitudes)
            {
                await solicitud.Contenido.DisposeAsync();
            }

            return ValidationProblem(ModelState);
        }

        try
        {
            var evidencias = await evidenciaInspeccionServicio.RegistrarAsync(
                ordenServicioId,
                solicitudes,
                cancellationToken);
            return StatusCode(StatusCodes.Status201Created, evidencias);
        }
        finally
        {
            foreach (var solicitud in solicitudes)
            {
                await solicitud.Contenido.DisposeAsync();
            }
        }
    }

    [HttpGet("evidencias/{evidenciaId:long}/contenido")]
    public async Task<IActionResult> ObtenerContenidoEvidencia(
        long ordenServicioId,
        long evidenciaId,
        CancellationToken cancellationToken)
    {
        var direccion = await evidenciaInspeccionServicio.CrearDireccionLecturaAsync(
            ordenServicioId,
            evidenciaId,
            cancellationToken);
        return Redirect(direccion.AbsoluteUri);
    }

    private static async Task<string?> ValidarFotografiaAsync(
        IFormFile fotografia,
        CancellationToken cancellationToken)
    {
        if (fotografia.Length == 0)
        {
            return $"'{fotografia.FileName}' está vacía.";
        }

        if (fotografia.Length > LongitudMaximaFotografia)
        {
            return $"'{fotografia.FileName}' supera el máximo de 10 MB.";
        }

        if (fotografia.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
        {
            await using var contenidoWebp = fotografia.OpenReadStream();
            var cabeceraWebp = new byte[12];
            if (await contenidoWebp.ReadAsync(cabeceraWebp, cancellationToken) != cabeceraWebp.Length ||
                !cabeceraWebp.AsSpan(0, 4).SequenceEqual("RIFF"u8) ||
                !cabeceraWebp.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            {
                return $"'{fotografia.FileName}' no contiene una imagen WebP válida.";
            }

            return null;
        }

        if (!FirmasPermitidas.TryGetValue(fotografia.ContentType, out var firma))
        {
            return $"'{fotografia.FileName}' debe ser una imagen JPEG, PNG o WebP.";
        }

        await using var contenido = fotografia.OpenReadStream();
        var cabecera = new byte[firma.Length];
        if (await contenido.ReadAsync(cabecera, cancellationToken) != firma.Length ||
            !cabecera.AsSpan().SequenceEqual(firma))
        {
            return $"'{fotografia.FileName}' no coincide con el formato declarado.";
        }

        return null;
    }
}
