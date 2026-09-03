using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talleres.Api.Autenticacion;
using Talleres.Aplicacion.DTOs.Autenticacion;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Dominio.Excepciones;

namespace Talleres.Api.Controllers;

[ApiController]
[Route("api/autenticacion")]
public sealed class AutenticacionController(
    IAutenticacionServicio autenticacionServicio,
    IConfiguration configuracion) : ControllerBase
{
    [HttpGet("externo/{proveedor}")]
    [AllowAnonymous]
    public IActionResult IniciarSesionExterna(string proveedor)
    {
        var esquema = proveedor.ToLowerInvariant() switch
        {
            "google" => "Google",
            "microsoft" => "Microsoft",
            _ => throw new AccesoDenegadoException("Proveedor de autenticación no admitido.")
        };
        var retorno = Url.ActionLink(nameof(CompletarSesionExterna), values: new { proveedor })!;
        return Challenge(new AuthenticationProperties { RedirectUri = retorno }, esquema);
    }

    [HttpGet("externo/{proveedor}/completar")]
    [AllowAnonymous]
    public async Task<IActionResult> CompletarSesionExterna(
        string proveedor,
        CancellationToken cancellationToken)
    {
        var resultado = await HttpContext.AuthenticateAsync("Talleres.Externo");
        if (!resultado.Succeeded || resultado.Principal is null)
        {
            return Redirect(ObtenerFrontal("error=autenticacion_externa"));
        }

        var clave = resultado.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var correo = resultado.Principal.FindFirstValue(ClaimTypes.Email)
            ?? resultado.Principal.FindFirstValue("email");
        if (string.IsNullOrWhiteSpace(clave))
        {
            return Redirect(ObtenerFrontal("error=identidad_sin_clave"));
        }

        var sesion = await autenticacionServicio.IniciarSesionExternaAsync(
            proveedor.Equals("microsoft", StringComparison.OrdinalIgnoreCase) ? "Microsoft" : "Google",
            clave,
            correo,
            cancellationToken);
        await EstablecerCookieAsync(sesion, true);
        await HttpContext.SignOutAsync("Talleres.Externo");
        return Redirect(ObtenerFrontal());
    }

    [HttpPost("iniciar")]
    [AllowAnonymous]
    [ProducesResponseType<SesionTallerDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SesionTallerDto>> IniciarSesion(
        InicioSesionSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var sesion = await autenticacionServicio.IniciarSesionAsync(
            solicitud,
            cancellationToken);
        await EstablecerCookieAsync(sesion, solicitud.Recordarme);
        return Ok(sesion);
    }

    [HttpGet("sesion")]
    [ProducesResponseType<SesionTallerDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SesionTallerDto>> ObtenerSesion(
        CancellationToken cancellationToken)
    {
        var sesion = await autenticacionServicio.ObtenerSesionAsync(
            ObtenerUsuarioId(),
            ObtenerEmpresaNovaId(),
            cancellationToken);
        return Ok(sesion);
    }

    [HttpPost("seleccionar-taller")]
    [ProducesResponseType<SesionTallerDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SesionTallerDto>> SeleccionarTaller(
        SeleccionarTallerSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var sesion = await autenticacionServicio.SeleccionarTallerAsync(
            ObtenerUsuarioId(),
            solicitud.EmpresaNovaId,
            cancellationToken);
        await EstablecerCookieAsync(sesion, false);
        return Ok(sesion);
    }

    [HttpPost("cerrar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CerrarSesion()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private string ObtenerUsuarioId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new AccesoDenegadoException("La sesión no contiene un usuario válido.");

    private int ObtenerEmpresaNovaId() =>
        int.TryParse(User.FindFirstValue(ReclamosSesion.EmpresaNovaId), out var empresaId)
            ? empresaId
            : throw new AccesoDenegadoException(
                "La sesión no contiene un taller válido.");

    private async Task EstablecerCookieAsync(
        SesionTallerDto sesion,
        bool persistente)
    {
        Claim[] reclamos =
        [
            new(ClaimTypes.NameIdentifier, sesion.UsuarioId),
            new(ClaimTypes.Name, sesion.NombreUsuario),
            new(ReclamosSesion.EmpresaNovaId, sesion.Taller.Id.ToString()),
            new(ReclamosSesion.EsSuperUsuario, sesion.EsSuperUsuario.ToString())
        ];
        var identidad = new ClaimsIdentity(
            reclamos,
            CookieAuthenticationDefaults.AuthenticationScheme);
        var propiedades = new AuthenticationProperties
        {
            IsPersistent = persistente,
            AllowRefresh = true,
            ExpiresUtc = persistente ? DateTimeOffset.UtcNow.AddDays(7) : null
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            propiedades);
    }

    private string ObtenerFrontal(string sufijo = "")
    {
        var frontal = configuracion["Autenticacion:FrontalUrl"]
            ?? "http://localhost:3000";
        return string.IsNullOrWhiteSpace(sufijo) ? frontal : $"{frontal}/?{sufijo}";
    }
}
