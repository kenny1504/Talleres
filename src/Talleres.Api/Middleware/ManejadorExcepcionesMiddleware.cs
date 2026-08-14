using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talleres.Dominio.Excepciones;

namespace Talleres.Api.Middleware;

public sealed class ManejadorExcepcionesMiddleware(
    RequestDelegate siguiente,
    ILogger<ManejadorExcepcionesMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await siguiente(contexto);
        }
        catch (Exception excepcion)
        {
            await EscribirRespuestaAsync(contexto, excepcion);
        }
    }

    private async Task EscribirRespuestaAsync(HttpContext contexto, Exception excepcion)
    {
        var (estado, titulo, detalle) = excepcion switch
        {
            RecursoNoEncontradoException => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                excepcion.Message),
            ReglaNegocioException => (
                StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio incumplida",
                excepcion.Message),
            DbUpdateException => (
                StatusCodes.Status409Conflict,
                "Conflicto al guardar",
                "La información entra en conflicto con un registro existente."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado al procesar la solicitud.")
        };

        if (estado >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(excepcion, "Error no controlado al procesar {Ruta}", contexto.Request.Path);
        }
        else
        {
            logger.LogInformation(
                excepcion,
                "Solicitud rechazada en {Ruta}: {Motivo}",
                contexto.Request.Path,
                excepcion.Message);
        }

        contexto.Response.StatusCode = estado;
        await contexto.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = detalle
            },
            contexto.RequestAborted);
    }
}
