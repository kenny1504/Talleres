using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.CatalogosVehiculos;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class CatalogoVehiculoServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : ICatalogoVehiculoServicio
{
    public async Task<IReadOnlyCollection<MarcaVehiculoDto>> ListarMarcasAsync(
        bool incluirInactivas = false,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var consulta = dbContext.MarcasVehiculo.AsNoTracking();
        if (!incluirInactivas)
        {
            consulta = consulta.Where(marca => marca.Activa);
        }

        return await consulta.OrderBy(marca => marca.Nombre)
            .Select(marca => new MarcaVehiculoDto(
                marca.Id,
                marca.Nombre,
                marca.Activa,
                marca.FechaCreacion))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<MarcaVehiculoDto> CrearMarcaAsync(
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreMarcaDisponibleAsync(nombre, null, cancellationToken);

        var marca = new MarcaVehiculo
        {
            EmpresaId = empresaId,
            Nombre = nombre,
            Activa = solicitud.Activa,
            FechaCreacion = DateTime.UtcNow
        };
        dbContext.MarcasVehiculo.Add(marca);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirMarca(marca);
    }

    public async Task<MarcaVehiculoDto> ActualizarMarcaAsync(
        long marcaId,
        GuardarMarcaVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var marca = await dbContext.MarcasVehiculo.SingleOrDefaultAsync(
                item => item.Id == marcaId,
                cancellationToken)
            ?? throw new RecursoNoEncontradoException("La marca solicitada no existe.");
        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreMarcaDisponibleAsync(nombre, marcaId, cancellationToken);
        marca.Nombre = nombre;
        marca.Activa = solicitud.Activa;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirMarca(marca);
    }

    public async Task<IReadOnlyCollection<ModeloVehiculoDto>> ListarModelosAsync(
        long? marcaId = null,
        bool incluirInactivos = false,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var consulta = dbContext.ModelosVehiculo.AsNoTracking();
        if (marcaId.HasValue)
        {
            consulta = consulta.Where(modelo => modelo.MarcaVehiculoId == marcaId.Value);
        }
        if (!incluirInactivos)
        {
            consulta = consulta.Where(modelo => modelo.Activo && modelo.Marca.Activa);
        }

        return await consulta
            .OrderBy(modelo => modelo.Marca.Nombre)
            .ThenBy(modelo => modelo.Nombre)
            .Select(modelo => new ModeloVehiculoDto(
                modelo.Id,
                modelo.MarcaVehiculoId,
                modelo.Marca.Nombre,
                modelo.Nombre,
                modelo.Activo,
                modelo.FechaCreacion))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ModeloVehiculoDto> CrearModeloAsync(
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var marca = await ObtenerMarcaActivaAsync(solicitud.MarcaVehiculoId, cancellationToken);
        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreModeloDisponibleAsync(marca.Id, nombre, null, cancellationToken);

        var modelo = new ModeloVehiculo
        {
            EmpresaId = empresaId,
            MarcaVehiculoId = marca.Id,
            Nombre = nombre,
            Activo = solicitud.Activo,
            FechaCreacion = DateTime.UtcNow
        };
        dbContext.ModelosVehiculo.Add(modelo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirModelo(modelo, marca.Nombre);
    }

    public async Task<ModeloVehiculoDto> ActualizarModeloAsync(
        long modeloId,
        GuardarModeloVehiculoSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var modelo = await dbContext.ModelosVehiculo.SingleOrDefaultAsync(
                item => item.Id == modeloId,
                cancellationToken)
            ?? throw new RecursoNoEncontradoException("El modelo solicitado no existe.");
        var marca = await ObtenerMarcaActivaAsync(solicitud.MarcaVehiculoId, cancellationToken);
        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreModeloDisponibleAsync(marca.Id, nombre, modeloId, cancellationToken);
        if (modelo.MarcaVehiculoId != marca.Id &&
            await dbContext.Vehiculos.AsNoTracking().AnyAsync(
                vehiculo => vehiculo.ModeloVehiculoId == modelo.Id,
                cancellationToken))
        {
            throw new ReglaNegocioException(
                "No se puede cambiar la marca de un modelo que ya está asignado a vehículos.");
        }
        modelo.MarcaVehiculoId = marca.Id;
        modelo.Nombre = nombre;
        modelo.Activo = solicitud.Activo;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ConvertirModelo(modelo, marca.Nombre);
    }

    private async Task<MarcaVehiculo> ObtenerMarcaActivaAsync(
        long marcaId,
        CancellationToken cancellationToken) =>
        await dbContext.MarcasVehiculo.SingleOrDefaultAsync(
            marca => marca.Id == marcaId && marca.Activa,
            cancellationToken)
        ?? throw new RecursoNoEncontradoException("La marca indicada no existe o está inactiva.");

    private async Task ValidarNombreMarcaDisponibleAsync(
        string nombre,
        long? marcaId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.MarcasVehiculo.AnyAsync(
                marca => marca.Id != marcaId && marca.Nombre == nombre,
                cancellationToken))
        {
            throw new ReglaNegocioException("Ya existe una marca con el nombre indicado.");
        }
    }

    private async Task ValidarNombreModeloDisponibleAsync(
        long marcaId,
        string nombre,
        long? modeloId,
        CancellationToken cancellationToken)
    {
        if (await dbContext.ModelosVehiculo.AnyAsync(
                modelo => modelo.Id != modeloId &&
                          modelo.MarcaVehiculoId == marcaId &&
                          modelo.Nombre == nombre,
                cancellationToken))
        {
            throw new ReglaNegocioException(
                "Ya existe un modelo con el nombre indicado para esa marca.");
        }
    }

    private static MarcaVehiculoDto ConvertirMarca(MarcaVehiculo marca) =>
        new(marca.Id, marca.Nombre, marca.Activa, marca.FechaCreacion);

    private static ModeloVehiculoDto ConvertirModelo(
        ModeloVehiculo modelo,
        string nombreMarca) =>
        new(
            modelo.Id,
            modelo.MarcaVehiculoId,
            nombreMarca,
            modelo.Nombre,
            modelo.Activo,
            modelo.FechaCreacion);

    private static string LimpiarNombre(string nombre) => nombre.Trim();
}
