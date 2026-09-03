using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talleres.Infraestructura.Integraciones.SmartNova;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

namespace Talleres.Pruebas.Integraciones;

public sealed class IdentidadSmartNovaPruebas
{
    [Fact]
    public async Task Autenticar_ValidaHashCompatibleConIdentity()
    {
        var opciones = new DbContextOptionsBuilder<SmartNovaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var contexto = new SmartNovaDbContext(opciones);
        var verificador = new PasswordHasher<UsuarioSmartNova>();
        var usuario = new UsuarioSmartNova
        {
            Id = "usuario-nova",
            UserName = "operador@example.com",
            NormalizedUserName = "OPERADOR@EXAMPLE.COM",
            Nombre = "Operador",
            IdEmpresa = 3071,
            IsActive = true
        };
        usuario.PasswordHash = verificador.HashPassword(usuario, "Clave-Segura-123");
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        var identidad = new IdentidadSmartNova(contexto, verificador);

        var autenticado = await identidad.AutenticarAsync(
            "operador@example.com",
            "Clave-Segura-123",
            CancellationToken.None);
        var rechazado = await identidad.AutenticarAsync(
            "operador@example.com",
            "incorrecta",
            CancellationToken.None);

        Assert.NotNull(autenticado);
        Assert.Equal(3071, autenticado.EmpresaId);
        Assert.Null(rechazado);
    }
}
