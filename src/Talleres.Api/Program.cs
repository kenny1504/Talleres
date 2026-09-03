using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Talleres.Api.Middleware;
using Talleres.Api.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.Abstracciones.Integraciones;
using Talleres.Aplicacion.Servicios;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Infraestructura.Persistencia;
using Talleres.Infraestructura.Integraciones.SmartNova;
using Talleres.Infraestructura.Integraciones.SmartNova.Entidades;

var builder = WebApplication.CreateBuilder(args);
CargarCadenaConexionDesdeArchivoEntornoLocal(builder);
const string politicaCorsFrontal = "FrontalWeb";
var cadenaConexion = ObtenerCadenaConexionRemota(builder.Configuration);
var cadenaConexionNova = ObtenerCadenaConexionNova(builder.Configuration);
var origenesPermitidos = builder.Configuration
    .GetSection("Cors:OrigenesPermitidos")
    .Get<string[]>() ?? [];

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services
    .AddControllers()
    .AddJsonOptions(opciones =>
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.Cookie.Name = "Talleres.Sesion";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.Cookie.SecurePolicy = builder.Configuration.GetValue(
            "Autenticacion:CookieSegura",
            !builder.Environment.IsDevelopment())
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.None;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
        opciones.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opciones.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddCookie("Talleres.Externo")
    .AddGoogle(GoogleDefaults.AuthenticationScheme, opciones =>
    {
        opciones.SignInScheme = "Talleres.Externo";
        opciones.ClientId = ObtenerConfiguracion(builder.Configuration, "Autenticacion:Google:ClientId", "GOOGLE_CLIENT_ID");
        opciones.ClientSecret = ObtenerConfiguracion(builder.Configuration, "Autenticacion:Google:ClientSecret", "GOOGLE_CLIENT_SECRET");
        opciones.CallbackPath = "/api/autenticacion/externo/google/callback";
        opciones.Events.OnRemoteFailure = contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return contexto.Response.WriteAsJsonAsync(new { error = contexto.Failure?.Message ?? "Fallo de Google" });
        };
    })
    .AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, opciones =>
    {
        opciones.SignInScheme = "Talleres.Externo";
        opciones.ClientId = ObtenerConfiguracion(builder.Configuration, "Autenticacion:Microsoft:ClientId", "MICROSOFT_CLIENT_ID");
        opciones.ClientSecret = ObtenerConfiguracion(builder.Configuration, "Autenticacion:Microsoft:ClientSecret", "MICROSOFT_CLIENT_SECRET");
        opciones.CallbackPath = "/api/autenticacion/externo/microsoft/callback";
        opciones.Events.OnRemoteFailure = contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return contexto.Response.WriteAsJsonAsync(new { error = contexto.Failure?.Message ?? "Fallo de Microsoft" });
        };
    });
builder.Services.AddAuthorization(opciones =>
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddCors(opciones =>
    opciones.AddPolicy(
        politicaCorsFrontal,
        politica => politica
            .WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));

builder.Services.AddScoped<IContextoEmpresa, ContextoEmpresaHttp>();
builder.Services.AddDbContext<TallerDbContext>(opciones =>
    opciones.UseSqlServer(
        cadenaConexion,
        opcionesSql => opcionesSql.EnableRetryOnFailure(
            maxRetryCount: 20,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));
builder.Services.AddScoped<ITallerDbContext>(proveedor =>
    proveedor.GetRequiredService<TallerDbContext>());
builder.Services.AddDbContext<SmartNovaDbContext>(opciones =>
    opciones.UseSqlServer(
        cadenaConexionNova,
        opcionesSql => opcionesSql.EnableRetryOnFailure()));
builder.Services.AddScoped<IPasswordHasher<UsuarioSmartNova>, PasswordHasher<UsuarioSmartNova>>();
builder.Services.AddScoped<IIdentidadSmartNova, IdentidadSmartNova>();
builder.Services.AddScoped<IInventarioSmartNova, InventarioSmartNova>();

builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddScoped<IOrdenServicioServicio, OrdenServicioServicio>();
builder.Services.AddScoped<IRecepcionVehiculoServicio, RecepcionVehiculoServicio>();
builder.Services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();
builder.Services.AddScoped<ITalleresSincronizadosServicio, TalleresSincronizadosServicio>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("BaseDatos:AplicarMigracionesAlIniciar"))
{
    await AplicarMigracionesAsync(
        app.Services,
        app.Lifetime.ApplicationStopping);
}

app.UseMiddleware<ManejadorExcepcionesMiddleware>();
if (builder.Configuration.GetValue("Http:UsarRedireccionHttps", true))
{
    app.UseHttpsRedirection();
}

app.UseCors(politicaCorsFrontal);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet(
    "/salud",
    async (
        TallerDbContext contexto,
        ILogger<Program> registro,
        CancellationToken cancellationToken) =>
    {
        using var limiteConexion = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        limiteConexion.CancelAfter(TimeSpan.FromSeconds(10));

        var baseDatosDisponible = false;
        try
        {
            baseDatosDisponible = await contexto.Database.CanConnectAsync(limiteConexion.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // El servidor remoto no respondió dentro del límite del chequeo de salud.
            registro.LogWarning(
                "La comprobación de salud agotó el tiempo de espera al conectar con TallerDb.");
        }
        catch (Exception excepcion) when (
            excepcion is not OperationCanceledException ||
            !cancellationToken.IsCancellationRequested)
        {
            registro.LogWarning(
                excepcion,
                "La comprobación de salud no pudo conectar con TallerDb.");
        }

        return baseDatosDisponible
            ? Results.Ok(new
            {
                estado = "saludable",
                baseDatos = "disponible",
                fechaUtc = DateTime.UtcNow
            })
            : Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Base de datos no disponible",
                detail: "La API no pudo establecer conexión con SQL Server.");
    }).AllowAnonymous();

app.Run();

static async Task AplicarMigracionesAsync(
    IServiceProvider proveedorServicios,
    CancellationToken cancellationToken)
{
    await using var alcance = proveedorServicios.CreateAsyncScope();
    var contexto = alcance.ServiceProvider.GetRequiredService<TallerDbContext>();
    await contexto.Database.MigrateAsync(cancellationToken);
}

static void CargarCadenaConexionDesdeArchivoEntornoLocal(WebApplicationBuilder builder)
{
    var conexionTallerConfigurada =
        !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("TallerDb")) ||
        !string.IsNullOrWhiteSpace(builder.Configuration["TALLERES_CONNECTION_STRING"]);
    var conexionNovaConfigurada =
        !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("SmartNova")) ||
        !string.IsNullOrWhiteSpace(builder.Configuration["SMART_NOVA_CONNECTION_STRING"]);
    if (!builder.Environment.IsDevelopment() ||
        (conexionTallerConfigurada && conexionNovaConfigurada))
    {
        return;
    }

    var archivoEntorno = BuscarArchivoEntornoDelRepositorio(
        builder.Environment.ContentRootPath,
        Directory.GetCurrentDirectory(),
        AppContext.BaseDirectory);
    if (archivoEntorno is null)
    {
        return;
    }

    var cadenaConexion = LeerValorArchivoEntorno(
        archivoEntorno,
        "TALLERES_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(cadenaConexion) &&
        string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("TallerDb")) &&
        string.IsNullOrWhiteSpace(builder.Configuration["TALLERES_CONNECTION_STRING"]))
    {
        builder.Configuration["TALLERES_CONNECTION_STRING"] = cadenaConexion;
    }

    var cadenaConexionNova = LeerValorArchivoEntorno(
        archivoEntorno,
        "SMART_NOVA_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(cadenaConexionNova) &&
        string.IsNullOrWhiteSpace(builder.Configuration["SMART_NOVA_CONNECTION_STRING"]))
    {
        builder.Configuration["SMART_NOVA_CONNECTION_STRING"] = cadenaConexionNova;
    }
}

static string? BuscarArchivoEntornoDelRepositorio(params string[] rutasIniciales)
{
    foreach (var rutaInicial in rutasIniciales.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        var directorio = new DirectoryInfo(rutaInicial);
        while (directorio is not null)
        {
            if (File.Exists(Path.Combine(directorio.FullName, "Talleres.sln")))
            {
                var archivoEntorno = Path.Combine(directorio.FullName, ".env");
                return File.Exists(archivoEntorno) ? archivoEntorno : null;
            }

            directorio = directorio.Parent;
        }
    }

    return null;
}

static string? LeerValorArchivoEntorno(string rutaArchivo, string claveBuscada)
{
    foreach (var linea in File.ReadLines(rutaArchivo))
    {
        var lineaLimpia = linea.Trim();
        if (lineaLimpia.Length == 0 || lineaLimpia.StartsWith('#'))
        {
            continue;
        }

        var separador = lineaLimpia.IndexOf('=');
        if (separador <= 0 ||
            !lineaLimpia[..separador].Trim().Equals(
                claveBuscada,
                StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        var valor = lineaLimpia[(separador + 1)..].Trim();
        if (valor.Length >= 2 &&
            ((valor[0] == '\'' && valor[^1] == '\'') ||
             (valor[0] == '"' && valor[^1] == '"')))
        {
            valor = valor[1..^1];
        }

        return valor;
    }

    return null;
}

static string ObtenerCadenaConexionRemota(IConfiguration configuracion)
{
    var cadenaConexion = configuracion.GetConnectionString("TallerDb")
        ?? configuracion["TALLERES_CONNECTION_STRING"];
    if (string.IsNullOrWhiteSpace(cadenaConexion))
    {
        throw new InvalidOperationException(
            "No se configuró la cadena remota mediante " +
            "'ConnectionStrings:TallerDb' o 'TALLERES_CONNECTION_STRING'.");
    }

    return ConfiguracionConexionSql.ValidarRemota(cadenaConexion);
}

static string ObtenerCadenaConexionNova(IConfiguration configuracion)
{
    var cadenaConexion = configuracion.GetConnectionString("SmartNova")
        ?? configuracion["SMART_NOVA_CONNECTION_STRING"];
    if (string.IsNullOrWhiteSpace(cadenaConexion))
    {
        throw new InvalidOperationException(
            "No se configuró SMART TPV NOVA mediante " +
            "'ConnectionStrings:SmartNova' o 'SMART_NOVA_CONNECTION_STRING'.");
    }

    return ConfiguracionConexionSql.ValidarRemota(cadenaConexion);
}

static string ObtenerConfiguracion(IConfiguration configuracion, string clave, string variableEntorno) =>
    configuracion[clave] ?? Environment.GetEnvironmentVariable(variableEntorno) ?? string.Empty;

public partial class Program;
