using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.EstadoNegocio;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/estado-negocio")]
public sealed class EstadoNegocioController(IEstadoNegocioServicio estadoNegocioServicio) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<EstadoNegocioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstadoNegocioDto>> Obtener(
        [FromQuery] ConsultarEstadoNegocioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await estadoNegocioServicio.ObtenerAsync(solicitud, cancellationToken));
    }

    [HttpGet("pagos")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType<PagosNegocioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagosNegocioDto>> ListarPagos(
        [FromQuery] ConsultarPagosNegocioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await estadoNegocioServicio.ListarPagosAsync(solicitud, cancellationToken));
    }
}
