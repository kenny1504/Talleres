using System.Text.Json.Serialization;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
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
using Talleres.Infraestructura.Integraciones.AmazonS3;
using Talleres.Dominio.Excepciones;

var builder = WebApplication.CreateBuilder(args);
CargarCadenaConexionDesdeArchivoEntornoLocal(builder);
CargarConfiguracionS3DesdeArchivoEntornoLocal(builder);
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
builder.Services.Configure<ForwardedHeadersOptions>(opciones =>
{
    opciones.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedHost |
        ForwardedHeaders.XForwardedProto;

    // La API solo está expuesta dentro de la red privada de Docker. El proxy web
    // es quien normaliza estos encabezados antes de reenviar cada solicitud.
    opciones.KnownNetworks.Clear();
    opciones.KnownProxies.Clear();
});
var autenticacion = builder.Services
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
    .AddCookie("Talleres.Externo");

var credencialesGoogle = ObtenerCredencialesProveedor(
    builder.Configuration,
    "Google",
    "GOOGLE_CLIENT_ID",
    "GOOGLE_CLIENT_SECRET");
if (credencialesGoogle is not null)
{
    autenticacion.AddGoogle(GoogleDefaults.AuthenticationScheme, opciones =>
    {
        opciones.SignInScheme = "Talleres.Externo";
        opciones.ClientId = credencialesGoogle.Value.ClientId;
        opciones.ClientSecret = credencialesGoogle.Value.ClientSecret;
        opciones.CallbackPath = "/api/autenticacion/externo/google/callback";
        opciones.Events.OnRemoteFailure = contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return contexto.Response.WriteAsJsonAsync(new { error = contexto.Failure?.Message ?? "Fallo de Google" });
        };
    });
}

var credencialesMicrosoft = ObtenerCredencialesProveedor(
    builder.Configuration,
    "Microsoft",
    "MICROSOFT_CLIENT_ID",
    "MICROSOFT_CLIENT_SECRET");
if (credencialesMicrosoft is not null)
{
    autenticacion.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, opciones =>
    {
        opciones.SignInScheme = "Talleres.Externo";
        opciones.ClientId = credencialesMicrosoft.Value.ClientId;
        opciones.ClientSecret = credencialesMicrosoft.Value.ClientSecret;
        opciones.CallbackPath = "/api/autenticacion/externo/microsoft/callback";
        opciones.Events.OnRemoteFailure = contexto =>
        {
            contexto.HandleResponse();
            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return contexto.Response.WriteAsJsonAsync(new { error = contexto.Failure?.Message ?? "Fallo de Microsoft" });
        };
    });
}
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
builder.Services.AddScoped<ICatalogoVehiculoServicio, CatalogoVehiculoServicio>();
builder.Services.AddScoped<IOrdenServicioServicio, OrdenServicioServicio>();
builder.Services.AddScoped<IDetalleOrdenServicio, DetalleOrdenServicio>();
builder.Services.AddScoped<IDiagnosticoOrdenServicioServicio, DiagnosticoOrdenServicioServicio>();
builder.Services.AddScoped<IRecepcionVehiculoServicio, RecepcionVehiculoServicio>();
builder.Services.AddScoped<IEvidenciaInspeccionServicio, EvidenciaInspeccionServicio>();
builder.Services.AddSingleton<IAlmacenamientoEvidencias>(_ =>
    new AlmacenamientoEvidenciasS3(
        () => CrearClienteS3(builder.Configuration),
        ObtenerConfiguracionS3(
            builder.Configuration,
            "UrlBucket",
            "AWS_S3_BUCKET")));
builder.Services.AddScoped<IAutenticacionServicio, AutenticacionServicio>();
builder.Services.AddScoped<ITalleresSincronizadosServicio, TalleresSincronizadosServicio>();

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("BaseDatos:AplicarMigracionesAlIniciar"))
{
    await AplicarMigracionesAsync(
        app.Services,
        app.Lifetime.ApplicationStopping);
}

app.UseForwardedHeaders();

var prefijoPublico = builder.Configuration["Proxy:PrefijoPublico"]?.TrimEnd('/');
if (!string.IsNullOrWhiteSpace(prefijoPublico))
{
    if (!prefijoPublico.StartsWith('/'))
    {
        throw new InvalidOperationException(
            "'Proxy:PrefijoPublico' debe comenzar con '/'.");
    }

    app.Use((contexto, siguiente) =>
    {
        contexto.Request.PathBase = prefijoPublico;
        return siguiente(contexto);
    });
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
        TallerDbContext contextoTalleres,
        SmartNovaDbContext contextoNova,
        ILogger<Program> registro,
        CancellationToken cancellationToken) =>
    {
        var comprobaciones = await Task.WhenAll(
            ComprobarConexionAsync(
                contextoTalleres,
                "TallerDb",
                registro,
                cancellationToken),
            ComprobarConexionAsync(
                contextoNova,
                "SmartNova",
                registro,
                cancellationToken));
        var talleresDisponible = comprobaciones[0];
        var novaDisponible = comprobaciones[1];
        var conexionesNoDisponibles = new List<string>(2);
        if (!talleresDisponible)
        {
            conexionesNoDisponibles.Add("TallerDb");
        }

        if (!novaDisponible)
        {
            conexionesNoDisponibles.Add("SmartNova");
        }

        return talleresDisponible && novaDisponible
            ? Results.Ok(new
            {
                estado = "saludable",
                baseDatosTalleres = "disponible",
                baseDatosNova = "disponible",
                fechaUtc = DateTime.UtcNow
            })
            : Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Bases de datos no disponibles",
                detail: $"La API no pudo establecer conexión con: {string.Join(", ", conexionesNoDisponibles)}.");
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

static async Task<bool> ComprobarConexionAsync(
    DbContext contexto,
    string nombreConexion,
    ILogger<Program> registro,
    CancellationToken cancellationToken)
{
    using var limiteConexion = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    limiteConexion.CancelAfter(TimeSpan.FromSeconds(10));

    try
    {
        var disponible = await contexto.Database.CanConnectAsync(limiteConexion.Token);
        if (!disponible)
        {
            registro.LogWarning(
                "La comprobación de salud indicó que {Conexion} no está disponible.",
                nombreConexion);
        }

        return disponible;
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        registro.LogWarning(
            "La comprobación de salud agotó el tiempo de espera al conectar con {Conexion}.",
            nombreConexion);
        return false;
    }
    catch (Exception excepcion) when (
        excepcion is not OperationCanceledException ||
        !cancellationToken.IsCancellationRequested)
    {
        registro.LogWarning(
            excepcion,
            "La comprobación de salud no pudo conectar con {Conexion}.",
            nombreConexion);
        return false;
    }
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

static void CargarConfiguracionS3DesdeArchivoEntornoLocal(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsDevelopment())
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

    foreach (var clave in new[]
             {
                 "AWS_S3_BUCKET",
                 "AWS_REGION",
                 "AWS_ACCESS_KEY_ID",
                 "AWS_SECRET_ACCESS_KEY",
                 "AWS_SESSION_TOKEN",
                 "AmazonS3__UrlBucket",
                 "AmazonS3__Region",
                 "AmazonS3__AwsAccesKey",
                 "AmazonS3__AwsSecretKey"
             })
    {
        if (string.IsNullOrWhiteSpace(builder.Configuration[clave]))
        {
            var valor = LeerValorArchivoEntorno(archivoEntorno, clave);
            if (string.IsNullOrWhiteSpace(valor))
            {
                continue;
            }

            builder.Configuration[clave.Replace("__", ":")] = valor;
        }
    }
}

static IAmazonS3 CrearClienteS3(IConfiguration configuracion)
{
    var region = ObtenerConfiguracionS3(configuracion, "Region", "AWS_REGION");
    if (string.IsNullOrWhiteSpace(region))
    {
        throw new IntegracionNoDisponibleException(
            "Amazon S3 no está configurado. Defina AWS_REGION.");
    }

    var opciones = new AmazonS3Config
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(region)
    };
    var accessKey = ObtenerConfiguracionS3(
        configuracion,
        "AwsAccesKey",
        "AWS_ACCESS_KEY_ID");
    var secretKey = ObtenerConfiguracionS3(
        configuracion,
        "AwsSecretKey",
        "AWS_SECRET_ACCESS_KEY");
    var sessionToken = configuracion["AWS_SESSION_TOKEN"];

    if (string.IsNullOrWhiteSpace(accessKey) && string.IsNullOrWhiteSpace(secretKey))
    {
        return new AmazonS3Client(opciones);
    }

    if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
    {
        throw new IntegracionNoDisponibleException(
            "Amazon S3 requiere configurar juntos AWS_ACCESS_KEY_ID y AWS_SECRET_ACCESS_KEY.");
    }

    AWSCredentials credenciales = string.IsNullOrWhiteSpace(sessionToken)
        ? new BasicAWSCredentials(accessKey, secretKey)
        : new SessionAWSCredentials(accessKey, secretKey, sessionToken);
    return new AmazonS3Client(credenciales, opciones);
}

static string? ObtenerConfiguracionS3(
    IConfiguration configuracion,
    string claveAmazonS3,
    string variableEntorno)
{
    var valorEntorno = configuracion[variableEntorno];
    return !string.IsNullOrWhiteSpace(valorEntorno)
        ? valorEntorno
        : configuracion[$"AmazonS3:{claveAmazonS3}"];
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

static (string ClientId, string ClientSecret)? ObtenerCredencialesProveedor(
    IConfiguration configuracion,
    string proveedor,
    string variableClientId,
    string variableClientSecret)
{
    var clientId = configuracion[$"Autenticacion:{proveedor}:ClientId"]
        ?? Environment.GetEnvironmentVariable(variableClientId);
    var clientSecret = configuracion[$"Autenticacion:{proveedor}:ClientSecret"]
        ?? Environment.GetEnvironmentVariable(variableClientSecret);
    var tieneClientId = !string.IsNullOrWhiteSpace(clientId);
    var tieneClientSecret = !string.IsNullOrWhiteSpace(clientSecret);

    if (!tieneClientId && !tieneClientSecret)
    {
        return null;
    }

    if (!tieneClientId || !tieneClientSecret)
    {
        throw new InvalidOperationException(
            $"La autenticación con {proveedor} requiere configurar " +
            $"'{variableClientId}' y '{variableClientSecret}'.");
    }

    return (clientId!, clientSecret!);
}

public partial class Program;
