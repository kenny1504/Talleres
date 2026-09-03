using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Dominio.Excepciones;
using Talleres.Dominio.Entidades;

namespace Talleres.Aplicacion.Servicios;

public sealed class TalleresSincronizadosServicio(
    ITallerDbContext contexto,
    IIdentidadSmartNova identidadSmartNova) : ITalleresSincronizadosServicio
{
    public async Task<IReadOnlyList<TallerSincronizadoDto>> ListarAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        await ValidarSuperusuarioAsync(usuarioId, cancellationToken);
        var configuraciones = await contexto.TalleresSincronizados
            .AsNoTracking()
            .OrderBy(taller => taller.EmpresaNovaId)
            .ToListAsync(cancellationToken);
        var empresas = await identidadSmartNova.ObtenerTalleresAsync(
            configuraciones.Select(taller => taller.EmpresaNovaId).ToArray(),
            cancellationToken);

        return configuraciones.Select(configuracion =>
        {
            var empresa = empresas.SingleOrDefault(item => item.Id == configuracion.EmpresaNovaId);
            return new TallerSincronizadoDto(
                configuracion.EmpresaNovaId,
                empresa?.NombreLegal ?? "Empresa no encontrada en NOVA",
                empresa?.NombreComercial,
                configuracion.Activo);
        }).ToList();
    }

    public async Task<TallerSincronizadoDto> AgregarAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        await ValidarSuperusuarioAsync(usuarioId, cancellationToken);
        var empresa = (await identidadSmartNova.ObtenerTalleresAsync(
            [empresaNovaId],
            cancellationToken)).SingleOrDefault()
            ?? throw new ReglaNegocioException(
                "La empresa indicada no existe en SMART TPV NOVA.");
        var configuracion = await contexto.TalleresSincronizados
            .SingleOrDefaultAsync(item => item.EmpresaNovaId == empresaNovaId, cancellationToken);

        if (configuracion is null)
        {
            configuracion = new TallerSincronizado
            {
                EmpresaNovaId = empresaNovaId,
                Activo = true,
                FechaConfiguracionUtc = DateTime.UtcNow
            };
            contexto.TalleresSincronizados.Add(configuracion);
        }
        else
        {
            configuracion.Activo = true;
            configuracion.FechaConfiguracionUtc = DateTime.UtcNow;
        }

        await contexto.SaveChangesAsync(cancellationToken);
        return new TallerSincronizadoDto(
            empresa.Id,
            empresa.NombreLegal,
            empresa.NombreComercial,
            true);
    }

    public async Task RetirarAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        await ValidarSuperusuarioAsync(usuarioId, cancellationToken);
        var configuracion = await contexto.TalleresSincronizados
            .SingleOrDefaultAsync(item => item.EmpresaNovaId == empresaNovaId, cancellationToken)
            ?? throw new RecursoNoEncontradoException(
                "El taller no está configurado en Talleres.");
        configuracion.Activo = false;
        configuracion.FechaConfiguracionUtc = DateTime.UtcNow;
        await contexto.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidarSuperusuarioAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        var usuario = await identidadSmartNova.ObtenerUsuarioActivoAsync(
            usuarioId,
            cancellationToken);
        if (usuario is not { EsSuperUsuario: true })
        {
            throw new AccesoDenegadoException(
                "Solo un superusuario puede administrar talleres sincronizados.");
        }
    }
}
