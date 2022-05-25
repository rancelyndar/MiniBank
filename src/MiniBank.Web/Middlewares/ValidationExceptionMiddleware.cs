using MiniBank.Core;

namespace MiniBank.Web.Middlewares;

public class ValidationExceptionMiddleware
{
    public readonly RequestDelegate next;

    public ValidationExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            context.Response.StatusCode = 400;

            await context.Response.WriteAsJsonAsync(new {Message = exception.Message});
        }
        catch (FluentValidation.ValidationException exception)
        {
            var errors = exception.Errors.Select(x => $"{x.ErrorMessage}");

            var errorMessage = string.Join(Environment.NewLine, errors);
            
            await context.Response.WriteAsJsonAsync(new {Message = errorMessage});
        }
    }
}