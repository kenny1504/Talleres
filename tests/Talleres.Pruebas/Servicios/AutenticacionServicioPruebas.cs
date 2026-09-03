using Microsoft.EntityFrameworkCore;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Aplicacion.Servicios;
using Talleres.Dominio.Excepciones;
using Talleres.Infraestructura.Persistencia;
using Talleres.Pruebas.Soporte;

namespace Talleres.Pruebas.Servicios;

public sealed class AutenticacionServicioPruebas
{
    private static readonly TallerSmartNovaDto Taller3071 = new(
        3071,
        "Loyal Car Services",
        "Loyal Car Services",
        505,
        "85926990",
        "0011812970032S",
        "taller@example.com",
        "Managua",
        "Managua",
        null,
        null,
        null,
        new TimeSpan(8, 0, 0),
        new TimeSpan(18, 0, 0));

    [Fact]
    public async Task IniciarSesion_PermiteUsuarioDeTallerSincronizado()
    {
        await using var contexto = CrearContexto();
        var identidad = new IdentidadNovaPrueba(
            new UsuarioSmartNovaDto("usuario-1", "maria", "María López", false, 3071),
            [Taller3071]);
        var servicio = new AutenticacionServicio(contexto, identidad);

        var sesion = await servicio.IniciarSesionAsync(
            new InicioSesionSolicitud { Usuario = "maria", Contrasena = "secreta" },
            CancellationToken.None);

        Assert.Equal(3071, sesion.Taller.Id);
        Assert.False(sesion.EsSuperUsuario);
        Assert.Single(sesion.TalleresDisponibles);
    }

    [Fact]
    public async Task IniciarSesion_DeniegaUsuarioCuyaEmpresaNoEsTaller()
    {
        await using var contexto = CrearContexto();
        var identidad = new IdentidadNovaPrueba(
            new UsuarioSmartNovaDto("usuario-2", "carlos", "Carlos Pérez", false, 99),
            [Taller3071]);
        var servicio = new AutenticacionServicio(contexto, identidad);

        var excepcion = await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            servicio.IniciarSesionAsync(
                new InicioSesionSolicitud { Usuario = "carlos", Contrasena = "secreta" },
                CancellationToken.None));

        Assert.Contains("no está configurada", excepcion.Message);
    }

    [Fact]
    public async Task SeleccionarTaller_RevalidaQueUsuarioSeaSuperUsuario()
    {
        await using var contexto = CrearContexto();
        var identidad = new IdentidadNovaPrueba(
            new UsuarioSmartNovaDto("usuario-1", "maria", "María López", false, 3071),
            [Taller3071]);
        var servicio = new AutenticacionServicio(contexto, identidad);

        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            servicio.SeleccionarTallerAsync("usuario-1", 3071, CancellationToken.None));
    }

    [Fact]
    public async Task IniciarSesion_PermiteSuperUsuarioSinEmpresaPropia()
    {
        await using var contexto = CrearContexto();
        var identidad = new IdentidadNovaPrueba(
            new UsuarioSmartNovaDto("super-1", "admin", "Administración", true, null),
            [Taller3071]);
        var servicio = new AutenticacionServicio(contexto, identidad);

        var sesion = await servicio.IniciarSesionAsync(
            new InicioSesionSolicitud { Usuario = "admin", Contrasena = "secreta" },
            CancellationToken.None);

        Assert.True(sesion.EsSuperUsuario);
        Assert.Equal(3071, sesion.Taller.Id);
    }

    private static TallerDbContext CrearContexto()
    {
        var opciones = new DbContextOptionsBuilder<TallerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var contexto = new TallerDbContext(opciones, new ContextoEmpresaPrueba(3071));
        contexto.Database.EnsureCreated();
        return contexto;
    }

    private sealed class IdentidadNovaPrueba(
        UsuarioSmartNovaDto usuario,
        IReadOnlyList<TallerSmartNovaDto> talleres) : IIdentidadSmartNova
    {
        public Task<UsuarioSmartNovaDto?> AutenticarAsync(
            string nombreUsuario,
            string contrasena,
            CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(usuario);

        public Task<UsuarioSmartNovaDto?> AutenticarProveedorAsync(
            string proveedor,
            string claveProveedor,
            string? correo,
            CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(usuario);

        public Task<UsuarioSmartNovaDto?> ObtenerUsuarioActivoAsync(
            string usuarioId,
            CancellationToken cancellationToken) => Task.FromResult<UsuarioSmartNovaDto?>(usuario);

        public Task<IReadOnlyList<TallerSmartNovaDto>> ObtenerTalleresAsync(
            IReadOnlyCollection<int> empresaIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TallerSmartNovaDto>>(
                talleres.Where(taller => empresaIds.Contains(taller.Id)).ToList());
    }
}
