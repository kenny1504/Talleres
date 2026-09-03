using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Excepciones;

namespace Talleres.Aplicacion.Servicios;

public sealed class AutenticacionServicio(
    ITallerDbContext contexto,
    IIdentidadSmartNova identidadSmartNova) : IAutenticacionServicio
{
    public async Task<SesionTallerDto> IniciarSesionAsync(
        InicioSesionSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var usuario = await identidadSmartNova.AutenticarAsync(
            solicitud.Usuario.Trim(),
            solicitud.Contrasena,
            cancellationToken);
        if (usuario is null)
        {
            throw new CredencialesInvalidasException();
        }

        var talleres = await ObtenerTalleresSincronizadosAsync(cancellationToken);
        var tallerSeleccionado = usuario.EsSuperUsuario
            ? SeleccionarTallerInicial(usuario, talleres)
            : SeleccionarTallerUsuario(usuario, talleres);

        return CrearSesion(usuario, tallerSeleccionado, talleres);
    }

    public async Task<SesionTallerDto> IniciarSesionExternaAsync(
        string proveedor,
        string claveProveedor,
        string? correo,
        CancellationToken cancellationToken)
    {
        var usuario = await identidadSmartNova.AutenticarProveedorAsync(
            proveedor, claveProveedor, correo, cancellationToken)
            ?? throw new CredencialesInvalidasException();
        var talleres = await ObtenerTalleresSincronizadosAsync(cancellationToken);
        var taller = usuario.EsSuperUsuario
            ? SeleccionarTallerInicial(usuario, talleres)
            : SeleccionarTallerUsuario(usuario, talleres);
        return CrearSesion(usuario, taller, talleres);
    }

    public async Task<SesionTallerDto> ObtenerSesionAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        var usuario = await ObtenerUsuarioVigenteAsync(usuarioId, cancellationToken);
        var talleres = await ObtenerTalleresSincronizadosAsync(cancellationToken);
        var taller = talleres.SingleOrDefault(item => item.Id == empresaNovaId)
            ?? throw new AccesoDenegadoException(
                "El taller de la sesión ya no está habilitado en el sistema.");

        if (!usuario.EsSuperUsuario && usuario.EmpresaId != empresaNovaId)
        {
            throw new AccesoDenegadoException(
                "El usuario no pertenece al taller activo.");
        }

        return CrearSesion(usuario, taller, talleres);
    }

    public async Task<SesionTallerDto> SeleccionarTallerAsync(
        string usuarioId,
        int empresaNovaId,
        CancellationToken cancellationToken)
    {
        var usuario = await ObtenerUsuarioVigenteAsync(usuarioId, cancellationToken);
        if (!usuario.EsSuperUsuario)
        {
            throw new AccesoDenegadoException(
                "Solo un superusuario puede cambiar el taller activo.");
        }

        var talleres = await ObtenerTalleresSincronizadosAsync(cancellationToken);
        var taller = talleres.SingleOrDefault(item => item.Id == empresaNovaId)
            ?? throw new AccesoDenegadoException(
                "La empresa seleccionada no está configurada como taller sincronizado.");

        return CrearSesion(usuario, taller, talleres);
    }

    private async Task<UsuarioSmartNovaDto> ObtenerUsuarioVigenteAsync(
        string usuarioId,
        CancellationToken cancellationToken) =>
        await identidadSmartNova.ObtenerUsuarioActivoAsync(usuarioId, cancellationToken)
        ?? throw new AccesoDenegadoException(
            "La cuenta ya no está activa en SMART TPV NOVA.");

    private async Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresSincronizadosAsync(
        CancellationToken cancellationToken)
    {
        var ids = await contexto.TalleresSincronizados
            .AsNoTracking()
            .Where(taller => taller.Activo)
            .Select(taller => taller.EmpresaNovaId)
            .ToListAsync(cancellationToken);

        return await identidadSmartNova.ObtenerTalleresAsync(ids, cancellationToken);
    }

    private static TallerSmartNovaDto SeleccionarTallerInicial(
        UsuarioSmartNovaDto usuario,
        IReadOnlyList<TallerSmartNovaDto> talleres)
    {
        if (talleres.Count == 0)
        {
            throw new AccesoDenegadoException(
                "No hay talleres sincronizados disponibles para ingresar.");
        }

        return talleres.FirstOrDefault(taller => taller.Id == usuario.EmpresaId)
            ?? talleres[0];
    }

    private static TallerSmartNovaDto SeleccionarTallerUsuario(
        UsuarioSmartNovaDto usuario,
        IReadOnlyList<TallerSmartNovaDto> talleres)
    {
        if (usuario.EmpresaId is null ||
            talleres.All(taller => taller.Id != usuario.EmpresaId.Value))
        {
            throw new AccesoDenegadoException(
                "Su empresa no está configurada como un taller autorizado.");
        }

        return talleres.Single(taller => taller.Id == usuario.EmpresaId.Value);
    }

    private static SesionTallerDto CrearSesion(
        UsuarioSmartNovaDto usuario,
        TallerSmartNovaDto taller,
        IReadOnlyList<TallerSmartNovaDto> talleres) =>
        new(
            usuario.Id,
            usuario.Usuario,
            usuario.Nombre,
            usuario.EsSuperUsuario,
            taller,
            usuario.EsSuperUsuario ? talleres : [taller]);
}
