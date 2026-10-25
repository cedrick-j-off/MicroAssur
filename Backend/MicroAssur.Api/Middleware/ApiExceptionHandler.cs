using System.Text.Json;
using MicroAssur.Api.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MicroAssur.Api.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable", "La ressource demandée n’existe pas."),
            RequestConflictException => (StatusCodes.Status409Conflict, "Conflit", exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, "Requête invalide", exception.Message),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requête invalide", "Le corps ou les paramètres de la requête sont invalides."),
            _ => (0, "", "")
        };

        if (status == 0)
            return false;

        logger.LogWarning(exception, "Erreur API traitée avec le statut {StatusCode}", status);
        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        await httpContext.Response.WriteAsync(json, cancellationToken);
        return true;
    }
}
