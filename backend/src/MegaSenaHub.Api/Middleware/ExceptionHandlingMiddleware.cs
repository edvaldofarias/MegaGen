using MegaSenaHub.Application.Exceptions;
using MegaSenaHub.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MegaSenaHub.Api.Middleware;

/// <summary>
/// Middleware global de tratamento de exceções.
/// Converte exceções conhecidas em ProblemDetails com status codes adequados.
/// Stack trace nunca é exposto em produção.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain Validation Error"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException or ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid Argument"),
            NotSupportedException => (StatusCodes.Status501NotImplemented, "Not Implemented"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        if (environment.IsDevelopment())
        {
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(problem);
    }
}
