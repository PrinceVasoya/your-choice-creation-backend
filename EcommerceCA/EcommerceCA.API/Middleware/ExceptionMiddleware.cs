using System.Net;
using System.Text.Json;
using EcommerceCA.Common.Exceptions;
using EcommerceCA.Common.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ValidationException = EcommerceCA.Common.Exceptions.ValidationException;

namespace EcommerceCA.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate               _next;
    private readonly ILogger<ExceptionMiddleware>  _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next   = next;
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
            _logger.LogError(ex,
                "Unhandled exception [{Method}] {Path} — {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (exception is StockValidationException stockEx)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400;
            return context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                Success = false,
                Message = stockEx.Message,
                StockErrors = stockEx.StockErrors,
                StatusCode = 400
            }, _jsonOptions));
        }

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException e     => (HttpStatusCode.NotFound,            e.Message, (IEnumerable<string>?)null),
            BadRequestException e   => (HttpStatusCode.BadRequest,          e.Message, null),
            UnauthorizedException e => (HttpStatusCode.Unauthorized,        e.Message, null),
            ForbiddenException e    => (HttpStatusCode.Forbidden,           e.Message, null),
            ConflictException e     => (HttpStatusCode.Conflict,            e.Message, null),
            ValidationException e   => (HttpStatusCode.UnprocessableEntity, e.Message,
                                        e.Errors.SelectMany(kvp => kvp.Value.Select(v => $"{kvp.Key}: {v}"))),
            _ when exception.GetType().FullName?.StartsWith("Razorpay.Api") == true
                                    => (HttpStatusCode.BadRequest,          "Razorpay error: " + exception.Message, null),
            _                       => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, (int)statusCode, errors);
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
    }
}
