using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ExceptionMiddewareSample;

public class ExceptionMiddlewareExample : IMiddleware
{
    private const string ERROR_MESSAGE = "An error occurred with the request. See the log for more details.";
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var response = context.Response;
        var statusCode = (int)HttpStatusCode.InternalServerError;
        response.ContentType = "application/json";

        try
        {
            await next(context);

            response.StatusCode = (int)HttpStatusCode.BadRequest;

            await response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { key = "Key", message = "Validation message" }));
            return;

        }
        catch (Exception exception)
        {
            var correlationId = response.Headers["X-Correlation-ID"].FirstOrDefault() ?? string.Empty;
            //Log.Error(exception, ERROR_MESSAGE);

            if (!response.HasStarted)
            {
                response.StatusCode = statusCode;
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(GetErrorMessage(correlationId)));
            }
        }

    }

    private static object GetErrorMessage(string correlationId)
    {
        object data = null;
        return new
        {
            data,
            errors = new[] { new { key = correlationId, message = ERROR_MESSAGE } }
        };
    }
}
