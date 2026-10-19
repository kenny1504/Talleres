using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Clientes;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class ClienteServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IClienteServicio
{
    public async Task EliminarAsync(long clienteId, CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes.SingleOrDefaultAsync(
            item => item.Id == clienteId && item.EmpresaId == empresaId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("El cliente solicitado no existe.");

        if (await dbContext.Vehiculos.AsNoTracking().AnyAsync(
                item => item.ClienteId == clienteId && item.EmpresaId == empresaId, cancellationToken))
        {
            throw new ReglaNegocioException("No se puede eliminar el cliente porque tiene vehículos asociados.");
        }

        if (await dbContext.OrdenesServicio.AsNoTracking().AnyAsync(
                item => item.ClienteId == clienteId && item.EmpresaId == empresaId, cancellationToken))
        {
            throw new ReglaNegocioException("No se puede eliminar el cliente porque tiene órdenes asociadas.");
        }

        if (await dbContext.PagosClientes.AsNoTracking().AnyAsync(
                item => item.ClienteId == clienteId && item.EmpresaId == empresaId, cancellationToken))
        {
            throw new ReglaNegocioException("No se puede eliminar el cliente porque conserva un historial de pagos.");
        }

        dbContext.Clientes.Remove(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ClienteDto> CrearAsync(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        await ValidarTelefonoDisponibleAsync(empresaId, solicitud.Telefono, null, cancellationToken);

        var cliente = new Cliente
        {
            EmpresaId = empresaId,
            Nombre = solicitud.Nombre.Trim(),
            Telefono = solicitud.Telefono.Trim(),
            Direccion = LimpiarOpcional(solicitud.Direccion),
            FechaCreacion = DateTime.UtcNow
        };

        dbContext.Clientes.Add(cliente);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ConvertirDto(cliente);
    }

    public async Task<ClienteDto> ActualizarAsync(
        long clienteId,
        ActualizarClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes.SingleOrDefaultAsync(
                item => item.EmpresaId == empresaId && item.Id == clienteId,
                cancellationToken)
            ?? throw new RecursoNoEncontradoException("El cliente solicitado no existe.");

        await ValidarTelefonoDisponibleAsync(empresaId, solicitud.Telefono, clienteId, cancellationToken);

        cliente.Nombre = solicitud.Nombre.Trim();
        cliente.Telefono = solicitud.Telefono.Trim();
        cliente.Direccion = LimpiarOpcional(solicitud.Direccion);
        cliente.Activo = solicitud.Activo;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirDto(cliente);
    }

    public async Task<ClienteDto> ObtenerPorIdAsync(
        long clienteId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await dbContext.Clientes
                   .AsNoTracking()
                   .Where(cliente => cliente.Id == clienteId)
                   .Select(cliente => new ClienteDto(
                       cliente.Id,
                       cliente.Nombre,
                       cliente.Telefono,
                       cliente.Direccion,
                       cliente.Activo,
                       cliente.FechaCreacion))
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new RecursoNoEncontradoException("El cliente solicitado no existe.");
    }

    public async Task<IReadOnlyCollection<ClienteDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await dbContext.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nombre)
            .Select(cliente => new ClienteDto(
                cliente.Id,
                cliente.Nombre,
                cliente.Telefono,
                cliente.Direccion,
                cliente.Activo,
                cliente.FechaCreacion))
            .ToArrayAsync(cancellationToken);
    }

    private async Task ValidarTelefonoDisponibleAsync(
        long empresaId,
        string telefono,
        long? clienteId,
        CancellationToken cancellationToken)
    {
        var telefonoNormalizado = Cliente.NormalizarTelefono(telefono);
        var clientesConTelefono = dbContext.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.EmpresaId == empresaId &&
                cliente.TelefonoNormalizado == telefonoNormalizado);

        if (clienteId is { } clienteActualId)
        {
            clientesConTelefono = clientesConTelefono.Where(cliente => cliente.Id != clienteActualId);
        }

        if (await clientesConTelefono.AnyAsync(cancellationToken))
        {
            throw new ReglaNegocioException("El cliente ya está registrado con este número telefónico.");
        }
    }

    private static ClienteDto ConvertirDto(Cliente cliente) => new(
        cliente.Id,
        cliente.Nombre,
        cliente.Telefono,
        cliente.Direccion,
        cliente.Activo,
        cliente.FechaCreacion);

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
