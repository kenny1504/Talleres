using Microsoft.AspNetCore.Mvc;

namespace Talleres.Api.Middleware;

public sealed class ValidacionEmpresaMiddleware(RequestDelegate siguiente)
{
    public const string NombreEncabezado = "X-Empresa-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        if (!HttpMethods.IsOptions(contexto.Request.Method) &&
            contexto.Request.Path.StartsWithSegments("/api") &&
            !EsEmpresaValida(contexto))
        {
            contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
            await contexto.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Empresa requerida",
                    Detail = $"Debe enviar el encabezado {NombreEncabezado} con un valor entero positivo."
                },
                contexto.RequestAborted);
            return;
        }

        await siguiente(contexto);
    }

    private static bool EsEmpresaValida(HttpContext contexto) =>
        long.TryParse(
            contexto.Request.Headers[NombreEncabezado].ToString(),
            out var empresaId) && empresaId > 0;
}
