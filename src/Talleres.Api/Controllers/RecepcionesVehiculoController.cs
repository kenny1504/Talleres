using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.Recepciones;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio/{ordenServicioId:long}/recepcion")]
public sealed class RecepcionesVehiculoController(
    IRecepcionVehiculoServicio recepcionVehiculoServicio) : ControllerBase
{
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
}
