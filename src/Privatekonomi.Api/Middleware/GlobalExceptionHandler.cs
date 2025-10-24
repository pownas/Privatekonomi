using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Privatekonomi.Api.Exceptions;
using System.Diagnostics;

namespace Privatekonomi.Api.Middleware;

/// <summary>
/// Global exception handler that converts exceptions to ProblemDetails responses.
/// Implements RFC 7807 Problem Details for HTTP APIs.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception),
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            Type = GetType(statusCode)
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Add validation errors if it's a ValidationException
        if (exception is ValidationException validationException && validationException.Errors.Any())
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        // Log the exception
        LogException(exception, statusCode, httpContext.TraceIdentifier);

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception) => exception switch
    {
        NotFoundException => StatusCodes.Status404NotFound,
        BadRequestException => StatusCodes.Status400BadRequest,
        ValidationException => StatusCodes.Status400BadRequest,
        ArgumentNullException => StatusCodes.Status400BadRequest,
        ArgumentException => StatusCodes.Status400BadRequest,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string GetTitle(Exception exception) => exception switch
    {
        NotFoundException => "Resource Not Found",
        BadRequestException => "Bad Request",
        ValidationException => "Validation Error",
        ArgumentNullException => "Invalid Argument",
        ArgumentException => "Invalid Argument",
        InvalidOperationException => "Invalid Operation",
        _ => "Internal Server Error"
    };

    private static string GetType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
        _ => "https://tools.ietf.org/html/rfc9110"
    };

    private void LogException(Exception exception, int statusCode, string traceId)
    {
        if (statusCode >= 500)
        {
            _logger.LogError(exception, 
                "Server error occurred. TraceId: {TraceId}", traceId);
        }
        else if (statusCode == 404)
        {
            _logger.LogWarning(exception, 
                "Resource not found. TraceId: {TraceId}", traceId);
        }
        else
        {
            _logger.LogInformation(exception, 
                "Client error occurred. TraceId: {TraceId}", traceId);
        }
    }
}
