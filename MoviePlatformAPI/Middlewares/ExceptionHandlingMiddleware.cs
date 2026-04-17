using System.Net;
using System.Text.Json;

namespace MoviePlatformAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unexpected error occurred."); 
            await HandleExceptionAsync(context, ex); 
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        int statusCode = (int)HttpStatusCode.InternalServerError;
        string message = "Internal Server Error";
        if (exception is UnauthorizedAccessException)
        {
            statusCode = (int)HttpStatusCode.Forbidden; 
            message = exception.Message; 
        }
        context.Response.StatusCode = statusCode;
        var response = new
        {
            StatusCode = statusCode,
            Message = message,
            Detailed = exception.Message 
        };
        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
     
    }

}