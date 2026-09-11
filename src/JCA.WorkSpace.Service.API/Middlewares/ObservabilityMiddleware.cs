using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using JCA.WorkSpace.Domain.Exceptions;

namespace JCA.WorkSpace.Service.API.Middlewares;

public class ObservabilityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ObservabilityMiddleware> _logger;

    public ObservabilityMiddleware(RequestDelegate next, ILogger<ObservabilityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var endpoint = $"{request.Method} {request.Path}";
        var requestId = Guid.NewGuid().ToString();

        context.Response.Headers["X-Request-Id"] = requestId;
        context.Items["RequestId"] = requestId;

        bool hasError = false;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            hasError = true;
            await HandleExceptionAsync(context, ex, stopwatch.ElapsedMilliseconds, endpoint, requestId);
        }
        finally
        {
            stopwatch.Stop();

            if (!hasError)
            {
                var statusCode = context.Response.StatusCode;
                if (statusCode >= 200 && statusCode < 300)
                {
                    _logger.LogInformation("Request Success | RequestId: {RequestId} | Endpoint: {Endpoint} | Status: {StatusCode} | Duration: {Duration}ms",
                        requestId, endpoint, statusCode, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("Request Non-Success | RequestId: {RequestId} | Endpoint: {Endpoint} | Status: {StatusCode} | Duration: {Duration}ms",
                        requestId, endpoint, statusCode, stopwatch.ElapsedMilliseconds);
                }
            }
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, long duration, string endpoint, string requestId)
    {
        int statusCode = StatusCodes.Status500InternalServerError;

        if (ex is ApiException apiEx)
        {
            statusCode = apiEx.StatusCode;
        }
        else if (ex is InvalidOperationException || ex is ArgumentException)
        {
            statusCode = StatusCodes.Status400BadRequest;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var responseBody = new
        {
            success = false,
            result = ex.Message
        };

        var json = JsonSerializer.Serialize(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _logger.LogError(ex, "Request Error | RequestId: {RequestId} | Endpoint: {Endpoint} | Status: {StatusCode} | Duration: {Duration}ms | Error: {Error}",
            requestId, endpoint, statusCode, duration, ex.Message);

        await context.Response.WriteAsync(json);
    }
}