using System.Net;
using System.Text.Json;
using CoffeeShop.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException notFound => (HttpStatusCode.NotFound, "Not Found", notFound.Message),
            UnauthorizedException unauthorized => (HttpStatusCode.Unauthorized, "Unauthorized", unauthorized.Message),
            BusinessException business => (HttpStatusCode.BadRequest, "Business Rule Violation", business.Message),
            ValidationException validation => (HttpStatusCode.BadRequest, "Validation Failed",
                string.Join("; ", validation.Errors.Select(e => e.ErrorMessage))),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", GetInternalErrorDetail(exception))
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private string GetInternalErrorDetail(Exception exception)
    {
        if (!_environment.IsDevelopment())
        {
            return "An unexpected error occurred.";
        }

        var message = exception.Message;
        if (exception.InnerException is not null)
        {
            message += $" | Inner: {exception.InnerException.Message}";
        }

        return message;
    }
}
