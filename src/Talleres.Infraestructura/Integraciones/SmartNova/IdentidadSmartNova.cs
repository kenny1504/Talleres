using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Infraestructura.Integraciones.SmartNova;

public sealed class IdentidadSmartNova(
    SmartNovaDbContext contexto,
    IPasswordHasher<UsuarioSmartNova> verificadorContrasena) : IIdentidadSmartNova
{
    public async Task<UsuarioSmartNovaDto?> AutenticarAsync(
        string usuario,
        string contrasena,
        CancellationToken cancellationToken)
    {
        try
        {
            var nombreNormalizado = usuario.ToUpperInvariant();
            var usuarioNova = await contexto.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.NormalizedUserName == nombreNormalizado && item.IsActive,
                    cancellationToken);

            if (usuarioNova?.PasswordHash is null)
            {
                return null;
            }

            var resultado = verificadorContrasena.VerifyHashedPassword(
                usuarioNova,
                usuarioNova.PasswordHash,
                contrasena);

            return resultado == PasswordVerificationResult.Failed
                ? null
                : ConvertirUsuario(usuarioNova);
        }
        catch (SqlException excepcion)
        {
            throw CrearExcepcionIntegracion(excepcion);
        }
    }

    public async Task<UsuarioSmartNovaDto?> ObtenerUsuarioActivoAsync(
        string usuarioId,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await contexto.Usuarios
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.Id == usuarioId && item.IsActive,
                    cancellationToken);
            return usuario is null ? null : ConvertirUsuario(usuario);
        }
        catch (SqlException excepcion)
        {
            throw CrearExcepcionIntegracion(excepcion);
        }
    }

    public async Task<UsuarioSmartNovaDto?> AutenticarProveedorAsync(
        string proveedor,
        string claveProveedor,
        string? correo,
        CancellationToken cancellationToken)
    {
        try
        {
            var usuarioId = await contexto.IniciosSesionExternos
                .AsNoTracking()
                .Where(item => item.LoginProvider == proveedor && item.ProviderKey == claveProveedor)
                .Select(item => item.UserId)
                .SingleOrDefaultAsync(cancellationToken);

            var usuario = usuarioId is null
                ? null
                : await contexto.Usuarios.AsNoTracking()
                    .SingleOrDefaultAsync(item => item.Id == usuarioId && item.IsActive, cancellationToken);

            if (usuario is null && !string.IsNullOrWhiteSpace(correo))
            {
                var correoNormalizado = correo.ToUpperInvariant();
                usuario = await contexto.Usuarios.AsNoTracking()
                    .SingleOrDefaultAsync(item => item.NormalizedUserName == correoNormalizado && item.IsActive, cancellationToken);
            }

            return usuario is null ? null : ConvertirUsuario(usuario);
        }
        catch (SqlException excepcion)
        {
            throw CrearExcepcionIntegracion(excepcion);
        }
    }

    public async Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresAsync(
        IReadOnlyCollection<int> empresaIds,
        CancellationToken cancellationToken)
    {
        if (empresaIds.Count == 0)
        {
            return [];
        }

        try
        {
            var empresas = await contexto.Empresas
                .AsNoTracking()
                .Where(empresa => empresaIds.Contains(empresa.Id))
                .OrderBy(empresa => empresa.NombreLegal)
                .ToListAsync(cancellationToken);

            return empresas
                .Select(empresa => new TallerSmartNovaDto(
                    empresa.Id,
                    empresa.NombreLegal,
                    empresa.NombreComercial,
                    empresa.PrefijoTelefono,
                    empresa.Telefono.ToString("0"),
                    empresa.Ruc,
                    empresa.Correo,
                    empresa.Dirreccion,
                    empresa.Ciudad,
                    empresa.Barrio,
                    empresa.Calle,
                    empresa.Logo,
                    empresa.HoraApertura,
                    empresa.HoraCierre))
                .ToList();
        }
        catch (SqlException excepcion)
        {
            throw CrearExcepcionIntegracion(excepcion);
        }
    }

    private static UsuarioSmartNovaDto ConvertirUsuario(UsuarioSmartNova usuario)
    {
        var nombreCompleto = string.Join(
            ' ',
            new[] { usuario.Nombre, usuario.Apellido }
                .Where(valor => !string.IsNullOrWhiteSpace(valor)))
            .Trim();

        return new UsuarioSmartNovaDto(
            usuario.Id,
            usuario.UserName ?? string.Empty,
            string.IsNullOrWhiteSpace(nombreCompleto)
                ? usuario.UserName ?? "Usuario"
                : nombreCompleto,
            usuario.EsSuperUser,
            usuario.IdEmpresa);
    }

    private static IntegracionNoDisponibleException CrearExcepcionIntegracion(
        SqlException excepcion) =>
        new(
            "No fue posible consultar SMART TPV NOVA en este momento.",
            excepcion);
}
