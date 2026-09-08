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

        var modelo = await ObtenerModeloActivoAsync(
            solicitud.ModeloVehiculoId,
            null,
            cancellationToken);

        var vehiculo = new Vehiculo
        {
            EmpresaId = empresaId,
            ClienteId = cliente.Id,
            Placa = placa,
            ModeloVehiculoId = modelo.Id,
            Anio = solicitud.Anio,
            Color = LimpiarOpcional(solicitud.Color),
            NumeroVin = LimpiarOpcional(solicitud.NumeroVin)?.ToUpperInvariant(),
            FechaCreacion = DateTime.UtcNow
        };

        dbContext.Vehiculos.Add(vehiculo);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ConvertirDto(vehiculo, cliente.Nombre, modelo);
    }

    public async Task<VehiculoDto> ActualizarAsync(
        long vehiculoId,
        ActualizarVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var vehiculo = await dbContext.Vehiculos.SingleOrDefaultAsync(
                item => item.Id == vehiculoId,
                cancellationToken)
            ?? throw new RecursoNoEncontradoException("El vehículo solicitado no existe.");

        var cliente = await dbContext.Clientes
                          .AsNoTracking()
                          .SingleOrDefaultAsync(
                              item => item.Id == solicitud.ClienteId && item.Activo,
                              cancellationToken)
                      ?? throw new RecursoNoEncontradoException(
                          "El cliente indicado no existe o está inactivo.");

        var placa = Normalizar(solicitud.Placa);
        if (await dbContext.Vehiculos.AnyAsync(
                item => item.Id != vehiculoId && item.Placa == placa,
                cancellationToken))
        {
            throw new ReglaNegocioException(
                "Ya existe otro vehículo con la placa indicada.");
        }

        var modelo = await ObtenerModeloActivoAsync(
            solicitud.ModeloVehiculoId,
            vehiculo.ModeloVehiculoId,
            cancellationToken);

        vehiculo.ClienteId = cliente.Id;
        vehiculo.Placa = placa;
        vehiculo.ModeloVehiculoId = modelo.Id;
        vehiculo.Anio = solicitud.Anio;
        vehiculo.Color = LimpiarOpcional(solicitud.Color);
        vehiculo.NumeroVin = LimpiarOpcional(solicitud.NumeroVin)?.ToUpperInvariant();
        vehiculo.Activo = solicitud.Activo;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirDto(vehiculo, cliente.Nombre, modelo);
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
            vehiculo.Modelo.MarcaVehiculoId,
            vehiculo.Modelo.Marca.Nombre,
            vehiculo.ModeloVehiculoId,
            vehiculo.Modelo.Nombre,
            vehiculo.Anio,
            vehiculo.Color,
            vehiculo.NumeroVin,
            vehiculo.Activo,
            vehiculo.FechaCreacion));

    private static VehiculoDto ConvertirDto(
        Vehiculo vehiculo,
        string nombreCliente,
        ModeloVehiculo modelo) => new(
        vehiculo.Id,
        vehiculo.ClienteId,
        nombreCliente,
        vehiculo.Placa,
        modelo.MarcaVehiculoId,
        modelo.Marca.Nombre,
        modelo.Id,
        modelo.Nombre,
        vehiculo.Anio,
        vehiculo.Color,
        vehiculo.NumeroVin,
        vehiculo.Activo,
        vehiculo.FechaCreacion);

    private async Task<ModeloVehiculo> ObtenerModeloActivoAsync(
        long modeloVehiculoId,
        long? modeloActualId,
        CancellationToken cancellationToken) =>
        await dbContext.ModelosVehiculo
            .AsNoTracking()
            .Include(modelo => modelo.Marca)
            .SingleOrDefaultAsync(
                modelo => modelo.Id == modeloVehiculoId &&
                          ((modelo.Activo && modelo.Marca.Activa) ||
                           modelo.Id == modeloActualId),
                cancellationToken)
        ?? throw new RecursoNoEncontradoException(
            "El modelo indicado no existe, está inactivo o su marca está inactiva.");

    private static string Normalizar(string valor) => valor.Trim().ToUpperInvariant();

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
