using System.Net;
using System.Text.Json;
using MoviePlatformAPI.Exceptions;

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
        string message = "An unexpected error occurred on the server.";
        string errorCode = "INTERNAL_SERVER_ERROR"; 

        switch (exception)
        {
            
            case BadRequestException badRequestEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = badRequestEx.Message;
                errorCode = badRequestEx.ErrorCode; 
                break;
                
            case UnauthorizedException unauthEx: 
                statusCode = (int)HttpStatusCode.Forbidden; 
                message = unauthEx.Message;
                errorCode = unauthEx.ErrorCode; 
                break;

            case NotFoundException notFoundEx: 
                statusCode = (int)HttpStatusCode.NotFound; 
                message = notFoundEx.Message;
                errorCode = notFoundEx.ErrorCode; 
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            ErrorCode = errorCode, 
            Message = message,
            Detailed = exception.Message 
        };

        var jsonResponse = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(jsonResponse);
    }
}