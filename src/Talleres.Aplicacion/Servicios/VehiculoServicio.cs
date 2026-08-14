using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Vehiculos;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class VehiculoServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : IVehiculoServicio
{
    public async Task<VehiculoDto> CrearAsync(
        CrearVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var cliente = await dbContext.Clientes
                          .AsNoTracking()
                          .SingleOrDefaultAsync(
                              item => item.Id == solicitud.ClienteId && item.Activo,
                              cancellationToken)
                      ?? throw new RecursoNoEncontradoException(
                          "El cliente indicado no existe o está inactivo.");

        var placa = Normalizar(solicitud.Placa);
        if (await dbContext.Vehiculos.AnyAsync(
                vehiculo => vehiculo.Placa == placa,
                cancellationToken))
        {
            throw new ReglaNegocioException(
                "Ya existe un vehículo con la placa indicada.");
        }

        var vehiculo = new Vehiculo
        {
            EmpresaId = empresaId,
            ClienteId = cliente.Id,
            Placa = placa,
            Marca = solicitud.Marca.Trim(),
            Modelo = solicitud.Modelo.Trim(),
            Anio = solicitud.Anio,
            Color = LimpiarOpcional(solicitud.Color),
            NumeroVin = LimpiarOpcional(solicitud.NumeroVin)?.ToUpperInvariant(),
            FechaCreacion = DateTime.UtcNow
        };

        dbContext.Vehiculos.Add(vehiculo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ConvertirDto(vehiculo, cliente.Nombre);
    }

    public async Task<VehiculoDto> ObtenerPorIdAsync(
        long vehiculoId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await ProyectarDto(
                       ConsultarVehiculos().Where(vehiculo => vehiculo.Id == vehiculoId))
                   .SingleOrDefaultAsync(cancellationToken)
               ?? throw new RecursoNoEncontradoException("El vehículo solicitado no existe.");
    }

    public async Task<IReadOnlyCollection<VehiculoDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await ProyectarDto(
                ConsultarVehiculos().OrderBy(vehiculo => vehiculo.Placa))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<VehiculoDto>> ListarPorClienteAsync(
        long clienteId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        if (!await dbContext.Clientes.AsNoTracking().AnyAsync(
                cliente => cliente.Id == clienteId,
                cancellationToken))
        {
            throw new RecursoNoEncontradoException("El cliente solicitado no existe.");
        }

        return await ProyectarDto(
                ConsultarVehiculos()
                    .Where(vehiculo => vehiculo.ClienteId == clienteId)
                    .OrderBy(vehiculo => vehiculo.Placa))
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<Vehiculo> ConsultarVehiculos() => dbContext.Vehiculos.AsNoTracking();

    private static IQueryable<VehiculoDto> ProyectarDto(IQueryable<Vehiculo> consulta) =>
        consulta.Select(vehiculo => new VehiculoDto(
            vehiculo.Id,
            vehiculo.ClienteId,
            vehiculo.Cliente.Nombre,
            vehiculo.Placa,
            vehiculo.Marca,
            vehiculo.Modelo,
            vehiculo.Anio,
            vehiculo.Color,
            vehiculo.NumeroVin,
            vehiculo.Activo,
            vehiculo.FechaCreacion));

    private static VehiculoDto ConvertirDto(Vehiculo vehiculo, string nombreCliente) => new(
        vehiculo.Id,
        vehiculo.ClienteId,
        nombreCliente,
        vehiculo.Placa,
        vehiculo.Marca,
        vehiculo.Modelo,
        vehiculo.Anio,
        vehiculo.Color,
        vehiculo.NumeroVin,
        vehiculo.Activo,
        vehiculo.FechaCreacion);

    private static string Normalizar(string valor) => valor.Trim().ToUpperInvariant();

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
