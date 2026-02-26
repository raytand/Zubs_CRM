using System.Net;
using System.Text.Json;
using Zubs.Application.Exceptions;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, ex.Message);
            await HandleAppException(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleUnknownException(context);
        }
    }

    private static Task HandleAppException(HttpContext context, AppException ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            NotFoundException => (int)HttpStatusCode.NotFound,
            ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.BadRequest
        };

        object response = ex switch
        {
            ValidationException ve => new
            {
                status = context.Response.StatusCode,
                message = ve.Message,
                errors = ve.Errors
            },
            _ => new
            {
                status = context.Response.StatusCode,
                message = ex.Message
            }
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleUnknownException(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = 500,
            message = "Internal server error"
        }));
    }
}
