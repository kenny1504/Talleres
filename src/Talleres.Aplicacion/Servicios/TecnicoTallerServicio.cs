using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.TecnicosTaller;
using Talleres.Aplicacion.Extensiones;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Entidades;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class TecnicoTallerServicio(
    ITallerDbContext dbContext,
    IContextoEmpresa contextoEmpresa) : ITecnicoTallerServicio
{
    public async Task<IReadOnlyCollection<TecnicoTallerDto>> ListarAsync(CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        return await dbContext.TecnicosTaller.AsNoTracking()
            .OrderBy(tecnico => tecnico.Nombre)
            .Select(tecnico => new TecnicoTallerDto(
                tecnico.Id, tecnico.Nombre, tecnico.Activo, tecnico.EsPredeterminado))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<TecnicoTallerDto> CrearAsync(
        CrearTecnicoTallerSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        var empresaId = contextoEmpresa.ObtenerEmpresaIdRequerido();
        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreDisponibleAsync(nombre, null, cancellationToken);
        var anterior = await dbContext.TecnicosTaller.SingleOrDefaultAsync(
            tecnico => tecnico.EsPredeterminado, cancellationToken);
        var tecnico = new TecnicoTaller
        {
            EmpresaId = empresaId,
            Nombre = nombre,
            Activo = true,
            EsPredeterminado = anterior is null,
            FechaCreacion = DateTime.UtcNow
        };

        if (anterior is not null && solicitud.EsPredeterminado)
        {
            await using var transaccion = await dbContext.IniciarTransaccionAsync(cancellationToken);
            anterior.EsPredeterminado = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            tecnico.EsPredeterminado = true;
            dbContext.TecnicosTaller.Add(tecnico);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
            return Convertir(tecnico);
        }

        dbContext.TecnicosTaller.Add(tecnico);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Convertir(tecnico);
    }

    public async Task<TecnicoTallerDto> ActualizarAsync(
        long tecnicoId,
        GuardarTecnicoTallerSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var tecnico = await ObtenerAsync(tecnicoId, cancellationToken);
        if (tecnico.EsPredeterminado && !solicitud.Activo)
            throw new ReglaNegocioException("Seleccione otro técnico predeterminado antes de desactivar este.");

        var nombre = LimpiarNombre(solicitud.Nombre);
        await ValidarNombreDisponibleAsync(nombre, tecnicoId, cancellationToken);
        tecnico.Nombre = nombre;
        tecnico.Activo = solicitud.Activo;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Convertir(tecnico);
    }

    public async Task<TecnicoTallerDto> EstablecerPredeterminadoAsync(
        long tecnicoId,
        CancellationToken cancellationToken = default)
    {
        contextoEmpresa.ObtenerEmpresaIdRequerido();
        var tecnico = await ObtenerAsync(tecnicoId, cancellationToken);
        if (!tecnico.Activo)
            throw new ReglaNegocioException("Un técnico inactivo no puede ser predeterminado.");
        if (tecnico.EsPredeterminado)
            return Convertir(tecnico);

        await using var transaccion = await dbContext.IniciarTransaccionAsync(cancellationToken);
        var anterior = await dbContext.TecnicosTaller.SingleOrDefaultAsync(
            item => item.EsPredeterminado, cancellationToken);
        if (anterior is not null)
        {
            anterior.EsPredeterminado = false;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        tecnico.EsPredeterminado = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaccion.CommitAsync(cancellationToken);
        return Convertir(tecnico);
    }

    private async Task<TecnicoTaller> ObtenerAsync(long tecnicoId, CancellationToken cancellationToken) =>
        await dbContext.TecnicosTaller.SingleOrDefaultAsync(
            tecnico => tecnico.Id == tecnicoId, cancellationToken)
        ?? throw new RecursoNoEncontradoException("El técnico solicitado no existe.");

    private async Task ValidarNombreDisponibleAsync(
        string nombre, long? tecnicoId, CancellationToken cancellationToken)
    {
        if (await dbContext.TecnicosTaller.AnyAsync(
                tecnico => tecnico.Nombre == nombre && tecnico.Id != tecnicoId,
                cancellationToken))
            throw new ReglaNegocioException("Ya existe un técnico con ese nombre en el taller.");
    }

    private static string LimpiarNombre(string nombre)
    {
        var limpio = nombre.Trim();
        if (limpio.Length is < 2 or > 150)
            throw new ReglaNegocioException("El nombre del técnico debe tener entre 2 y 150 caracteres.");
        return limpio;
    }

    private static TecnicoTallerDto Convertir(TecnicoTaller tecnico) =>
        new(tecnico.Id, tecnico.Nombre, tecnico.Activo, tecnico.EsPredeterminado);
}
