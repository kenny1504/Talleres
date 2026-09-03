using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Dominio.Excepciones;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/talleres-sincronizados")]
public sealed class TalleresSincronizadosController(
    ITalleresSincronizadosServicio servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TallerSincronizadoDto>>> Listar(
        CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(ObtenerUsuarioId(), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<TallerSincronizadoDto>> Agregar(
        ConfigurarTallerSolicitud solicitud,
        CancellationToken cancellationToken) =>
        Ok(await servicio.AgregarAsync(
            ObtenerUsuarioId(), solicitud.EmpresaNovaId, cancellationToken));

    [HttpDelete("{empresaNovaId:int}")]
    public async Task<IActionResult> Retirar(
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        await servicio.RetirarAsync(
            ObtenerUsuarioId(), empresaNovaId, cancellationToken);
        return NoContent();
    }

    private string ObtenerUsuarioId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new AccesoDenegadoException("La sesión no contiene un usuario válido.");
}
