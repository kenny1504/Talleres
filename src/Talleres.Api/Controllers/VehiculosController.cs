using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.Vehiculos;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/vehiculos")]
public sealed class VehiculosController(IVehiculoServicio vehiculoServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<VehiculoDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var vehiculos = await vehiculoServicio.ListarAsync(cancellationToken);
        return Ok(vehiculos);
    }

    [HttpGet("{vehiculoId:long}")]
    [ProducesResponseType<VehiculoDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<VehiculoDto>> ObtenerPorId(
        long vehiculoId,
        CancellationToken cancellationToken)
    {
        var vehiculo = await vehiculoServicio.ObtenerPorIdAsync(
            vehiculoId,
            cancellationToken);
        return Ok(vehiculo);
    }

    [HttpGet("por-cliente/{clienteId:long}")]
    [ProducesResponseType<IReadOnlyCollection<VehiculoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<VehiculoDto>>> ListarPorCliente(
        long clienteId,
        CancellationToken cancellationToken)
    {
        var vehiculos = await vehiculoServicio.ListarPorClienteAsync(
            clienteId,
            cancellationToken);
        return Ok(vehiculos);
    }

    [HttpPost]
    [ProducesResponseType<VehiculoDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VehiculoDto>> Crear(
        CrearVehiculoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var vehiculo = await vehiculoServicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { vehiculoId = vehiculo.Id }, vehiculo);
    }
}
