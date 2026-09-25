using Microsoft.AspNetCore.Mvc;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.Servicios.Contratos;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController(
    IClienteServicio clienteServicio,
    IEstadoCuentaClienteServicio estadoCuentaClienteServicio) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ClienteDto>>> Listar(
        CancellationToken cancellationToken)
    {
        var clientes = await clienteServicio.ListarAsync(cancellationToken);
        return Ok(clientes);
    }

    [HttpGet("{clienteId:long}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> ObtenerPorId(
        long clienteId,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteServicio.ObtenerPorIdAsync(clienteId, cancellationToken);
        return Ok(cliente);
    }

    [HttpPost]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteDto>> Crear(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteServicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { clienteId = cliente.Id }, cliente);
    }

    [HttpPut("{clienteId:long}")]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ClienteDto>> Actualizar(
        long clienteId,
        ActualizarClienteSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteServicio.ActualizarAsync(
            clienteId,
            solicitud,
            cancellationToken);
        return Ok(cliente);
    }

    [HttpGet("{clienteId:long}/estado-cuenta")]
    [ProducesResponseType<EstadoCuentaClienteDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoCuentaClienteDto>> ObtenerEstadoCuenta(
        long clienteId,
        CancellationToken cancellationToken)
    {
        return Ok(await estadoCuentaClienteServicio.ObtenerAsync(clienteId, cancellationToken));
    }

    [HttpPost("{clienteId:long}/pagos")]
    [ProducesResponseType<EstadoCuentaClienteDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoCuentaClienteDto>> RegistrarPago(
        long clienteId,
        RegistrarPagoClienteSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await estadoCuentaClienteServicio.RegistrarPagoAsync(
            clienteId, solicitud, cancellationToken));
    }

    [HttpPost("{clienteId:long}/pagos/{pagoId:long}/anular")]
    [ProducesResponseType<EstadoCuentaClienteDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<EstadoCuentaClienteDto>> AnularPago(
        long clienteId,
        long pagoId,
        CancellationToken cancellationToken)
    {
        return Ok(await estadoCuentaClienteServicio.AnularPagoAsync(
            clienteId, pagoId, cancellationToken));
    }
}
