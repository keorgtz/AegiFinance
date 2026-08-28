using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Web.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = MapException(exception);

        var traceId = httpContext.TraceIdentifier;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada en {Path}. Referencia: {TraceId}", httpContext.Request.Path, traceId);
        }
        else if (exception is DbUpdateException)
        {
            _logger.LogWarning(exception, "Conflicto de persistencia en {Path}. Referencia: {TraceId}", httpContext.Request.Path, traceId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception switch
            {
                DbUpdateException => "No se pudo guardar porque los datos relacionados cambiaron o ya existe un registro equivalente. Actualiza la pantalla, verifica los datos e inténtalo de nuevo.",
                _ when statusCode == StatusCodes.Status500InternalServerError => "Ocurrió un error inesperado. Comunica la referencia mostrada para localizar el fallo en los registros del servidor.",
                _ => exception.Message
            },
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, IDictionary<string, string[]>? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "No autorizado", null),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "No encontrado", null),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Solicitud inválida", null),
            DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto al guardar", null),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor", null)
        };
    }
}
