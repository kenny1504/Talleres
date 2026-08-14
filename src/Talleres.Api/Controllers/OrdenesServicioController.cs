using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio")]
public sealed class OrdenesServicioController(
    IOrdenServicioServicio ordenServicioServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<OrdenServicioDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<OrdenServicioDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var ordenes = await ordenServicioServicio.ListarAsync(cancellationToken);
        return Ok(ordenes);
    }

    [HttpGet("{ordenServicioId:long}")]
    [ProducesResponseType<OrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrdenServicioDto>> ObtenerPorId(
        long ordenServicioId,
        CancellationToken cancellationToken)
    {
        var orden = await ordenServicioServicio.ObtenerPorIdAsync(
            ordenServicioId,
            cancellationToken);
        return Ok(orden);
    }

    [HttpPost]
    [ProducesResponseType<OrdenServicioDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrdenServicioDto>> Crear(
        CrearOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var orden = await ordenServicioServicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { ordenServicioId = orden.Id },
            orden);
    }

    [HttpPut("{ordenServicioId:long}/estado")]
    [ProducesResponseType<OrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrdenServicioDto>> CambiarEstado(
        long ordenServicioId,
        CambiarEstadoOrdenServicioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var orden = await ordenServicioServicio.CambiarEstadoAsync(
            ordenServicioId,
            solicitud,
            cancellationToken);
        return Ok(orden);
    }
}
