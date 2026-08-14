using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Talleres.Api.Middleware;
using Talleres.Api.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Multitenencia;
using Talleres.Aplicacion.Abstracciones.Persistencia;
using Talleres.Aplicacion.Servicios;
using Talleres.Aplicacion.Servicios.Contratos;
using Talleres.Infraestructura.Persistencia;

var builder = WebApplication.CreateBuilder(args);
CargarCadenaConexionDesdeArchivoEntornoLocal(builder);
const string politicaCorsFrontal = "FrontalWeb";
var cadenaConexion = ObtenerCadenaConexionRemota(builder.Configuration);
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
builder.Services.AddCors(opciones =>
    opciones.AddPolicy(
        politicaCorsFrontal,
        politica => politica
            .WithOrigins(origenesPermitidos)
            .AllowAnyHeader()
            .AllowAnyMethod()));

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

builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IVehiculoServicio, VehiculoServicio>();
builder.Services.AddScoped<IOrdenServicioServicio, OrdenServicioServicio>();
builder.Services.AddScoped<IRecepcionVehiculoServicio, RecepcionVehiculoServicio>();

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
app.UseMiddleware<ValidacionEmpresaMiddleware>();
app.MapControllers();
app.MapGet(
    "/salud",
    async (TallerDbContext contexto, CancellationToken cancellationToken) =>
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
    });

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
    if (!builder.Environment.IsDevelopment() ||
        !string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("TallerDb")) ||
        !string.IsNullOrWhiteSpace(builder.Configuration["TALLERES_CONNECTION_STRING"]))
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
    if (!string.IsNullOrWhiteSpace(cadenaConexion))
    {
        builder.Configuration["TALLERES_CONNECTION_STRING"] = cadenaConexion;
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

public partial class Program;
