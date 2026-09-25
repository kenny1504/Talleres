using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.TecnicosTaller;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/tecnicos-taller")]
public sealed class TecnicosTallerController(ITecnicoTallerServicio servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TecnicoTallerDto>>> Listar(CancellationToken cancellationToken) =>
        Ok(await servicio.ListarAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<TecnicoTallerDto>> Crear(
        CrearTecnicoTallerSolicitud solicitud, CancellationToken cancellationToken)
    {
        var tecnico = await servicio.CrearAsync(solicitud, cancellationToken);
        return Created($"api/tecnicos-taller/{tecnico.Id}", tecnico);
    }

    [HttpPut("{tecnicoId:long}")]
    public async Task<ActionResult<TecnicoTallerDto>> Actualizar(
        long tecnicoId, GuardarTecnicoTallerSolicitud solicitud, CancellationToken cancellationToken) =>
        Ok(await servicio.ActualizarAsync(tecnicoId, solicitud, cancellationToken));

    [HttpPut("{tecnicoId:long}/predeterminado")]
    public async Task<ActionResult<TecnicoTallerDto>> EstablecerPredeterminado(
        long tecnicoId, CancellationToken cancellationToken) =>
        Ok(await servicio.EstablecerPredeterminadoAsync(tecnicoId, cancellationToken));
}
