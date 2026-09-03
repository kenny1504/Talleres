using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres.Api.Autenticacion;
using Talleres.Aplicacion.Abstracciones.Integraciones;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public sealed class InventarioController(IInventarioSmartNova inventario) : ControllerBase
{
    [HttpGet("bodegas")]
    public async Task<IActionResult> ObtenerBodegas(CancellationToken cancellationToken)
    {
        var empresaId = ObtenerEmpresa();
        return Ok(await inventario.ObtenerBodegasAsync(empresaId, cancellationToken));
    }

    [HttpGet("existencias")]
    public async Task<IActionResult> ObtenerExistencias([FromQuery] int bodegaId, [FromQuery] string? criterio, CancellationToken cancellationToken)
    {
        var empresaId = ObtenerEmpresa();
        return Ok(await inventario.ObtenerExistenciasAsync(empresaId, bodegaId, criterio, cancellationToken));
    }

    private int ObtenerEmpresa() => int.Parse(User.FindFirst(ReclamosSesion.EmpresaNovaId)?.Value ?? "0");
}
