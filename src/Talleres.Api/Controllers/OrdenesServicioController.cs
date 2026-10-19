using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Talleres.Api.Autenticacion;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Excepciones;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio")]
public sealed class OrdenesServicioController(
    IOrdenServicioServicio ordenServicioServicio,
    IEliminacionOrdenServicio eliminacionOrdenServicio) : ControllerBase
{
    [HttpDelete("{ordenServicioId:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Eliminar(long ordenServicioId, CancellationToken cancellationToken)
    {
        var empresaNovaId = int.TryParse(User.FindFirstValue(ReclamosSesion.EmpresaNovaId), out var empresaId) && empresaId > 0
            ? empresaId : throw new AccesoDenegadoException("La sesión no contiene un taller válido.");
        var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new AccesoDenegadoException("La sesión no contiene un usuario válido.");
        await eliminacionOrdenServicio.EliminarAsync(ordenServicioId, empresaNovaId, usuarioId, cancellationToken);
        return NoContent();
    }

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

    [HttpPut("{ordenServicioId:long}/tecnico")]
    [ProducesResponseType<OrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<OrdenServicioDto>> AsignarTecnico(
        long ordenServicioId,
        AsignarTecnicoOrdenSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var orden = await ordenServicioServicio.AsignarTecnicoAsync(
            ordenServicioId, solicitud, cancellationToken);
        return Ok(orden);
    }
}
