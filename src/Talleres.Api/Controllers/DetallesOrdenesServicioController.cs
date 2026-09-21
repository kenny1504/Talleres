using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Talleres.Api.Autenticacion;
using Talleres.Aplicacion.DTOs.OrdenesServicio;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Excepciones;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/ordenes-servicio/{ordenServicioId:long}/detalles")]
public sealed class DetallesOrdenesServicioController(
    IDetalleOrdenServicio detalleOrdenServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ResumenDetallesOrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDetallesOrdenServicioDto>> Obtener(
        long ordenServicioId,
        CancellationToken cancellationToken)
    {
        var resumen = await detalleOrdenServicio.ObtenerAsync(
            ordenServicioId,
            cancellationToken);
        return Ok(resumen);
    }

    [HttpPost("inventario")]
    [ProducesResponseType<ResumenDetallesOrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDetallesOrdenServicioDto>> AgregarInventario(
        long ordenServicioId,
        AgregarDetalleInventarioSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var resumen = await detalleOrdenServicio.AgregarInventarioAsync(
            ordenServicioId,
            solicitud,
            ObtenerEmpresaNovaId(),
            ObtenerUsuarioId(),
            cancellationToken);
        return Ok(resumen);
    }

    [HttpPost("manual")]
    [ProducesResponseType<ResumenDetallesOrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDetallesOrdenServicioDto>> AgregarManual(
        long ordenServicioId,
        AgregarDetalleManualSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var resumen = await detalleOrdenServicio.AgregarManualAsync(
            ordenServicioId,
            solicitud,
            cancellationToken);
        return Ok(resumen);
    }

    [HttpDelete("{detalleId:long}")]
    [ProducesResponseType<ResumenDetallesOrdenServicioDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenDetallesOrdenServicioDto>> Eliminar(
        long ordenServicioId,
        long detalleId,
        CancellationToken cancellationToken)
    {
        var resumen = await detalleOrdenServicio.EliminarAsync(
            ordenServicioId,
            detalleId,
            ObtenerEmpresaNovaId(),
            ObtenerUsuarioId(),
            cancellationToken);
        return Ok(resumen);
    }

    private int ObtenerEmpresaNovaId() =>
        int.TryParse(User.FindFirstValue(ReclamosSesion.EmpresaNovaId), out var empresaId) &&
        empresaId > 0
            ? empresaId
            : throw new AccesoDenegadoException(
                "La sesión no contiene un taller válido.");

    private string ObtenerUsuarioId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new AccesoDenegadoException(
            "La sesión no contiene un usuario válido.");
}
