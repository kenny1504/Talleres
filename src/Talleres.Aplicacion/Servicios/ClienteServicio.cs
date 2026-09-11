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
    public async Task<ClienteDto> CrearAsync(
        CrearClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
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
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes.SingleOrDefaultAsync(
                item => item.Id == clienteId,
                cancellationToken)
            ?? throw new RecursoNoEncontradoException("El cliente solicitado no existe.");

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
