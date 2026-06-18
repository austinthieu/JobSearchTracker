using System.Net;
using System.Text.Json;
using FluentValidation;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            var response = new { errors = ex.Errors.Select(e => e.ErrorMessage) };
            await WriteJson(context, response);
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await WriteJson(context, new { error = ex.Message });
        }
        catch (Exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await WriteJson(context, new { error = "An internal error occurred" });
        }
    }

    private static async Task WriteJson(HttpContext context, object response)
    {
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
