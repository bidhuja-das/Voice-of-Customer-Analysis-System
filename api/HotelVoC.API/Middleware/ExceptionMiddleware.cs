using System.Net;
using System.Text.Json;

namespace HotelVoC.API.Middleware;

public class ExceptionMiddleware
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Something went wrong. Please try again.",
            detail = ex.Message
        };

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}