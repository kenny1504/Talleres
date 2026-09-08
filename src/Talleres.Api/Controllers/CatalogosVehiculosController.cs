using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.CatalogosVehiculos;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/catalogos-vehiculos")]
public sealed class CatalogosVehiculosController(
    ICatalogoVehiculoServicio catalogoVehiculoServicio) : ControllerBase
{
    [HttpGet("marcas")]
    public async Task<ActionResult<IReadOnlyCollection<MarcaVehiculoDto>>> ListarMarcas(
        [FromQuery] bool incluirInactivas,
        CancellationToken cancellationToken)
    {
        var marcas = await catalogoVehiculoServicio.ListarMarcasAsync(
            incluirInactivas,
            cancellationToken);
        return Ok(marcas);
    }

    [HttpPost("marcas")]
    public async Task<ActionResult<MarcaVehiculoDto>> CrearMarca(
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var marca = await catalogoVehiculoServicio.CrearMarcaAsync(solicitud, cancellationToken);
        return Created($"api/catalogos-vehiculos/marcas/{marca.Id}", marca);
    }

    [HttpPut("marcas/{marcaId:long}")]
    public async Task<ActionResult<MarcaVehiculoDto>> ActualizarMarca(
        long marcaId,
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var marca = await catalogoVehiculoServicio.ActualizarMarcaAsync(
            marcaId,
            solicitud,
            cancellationToken);
        return Ok(marca);
    }

    [HttpGet("modelos")]
    public async Task<ActionResult<IReadOnlyCollection<ModeloVehiculoDto>>> ListarModelos(
        [FromQuery] long? marcaId,
        [FromQuery] bool incluirInactivos,
        CancellationToken cancellationToken)
    {
        var modelos = await catalogoVehiculoServicio.ListarModelosAsync(
            marcaId,
            incluirInactivos,
            cancellationToken);
        return Ok(modelos);
    }

    [HttpPost("modelos")]
    public async Task<ActionResult<ModeloVehiculoDto>> CrearModelo(
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var modelo = await catalogoVehiculoServicio.CrearModeloAsync(solicitud, cancellationToken);
        return Created($"api/catalogos-vehiculos/modelos/{modelo.Id}", modelo);
    }

    [HttpPut("modelos/{modeloId:long}")]
    public async Task<ActionResult<ModeloVehiculoDto>> ActualizarModelo(
        long modeloId,
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var modelo = await catalogoVehiculoServicio.ActualizarModeloAsync(
            modeloId,
            solicitud,
            cancellationToken);
        return Ok(modelo);
    }
}
