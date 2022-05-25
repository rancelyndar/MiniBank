namespace MiniBank.Web.Middlewares;

public class ExceptionMiddleware
{
    public readonly RequestDelegate next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            context.Response.StatusCode = 500;
            
            await context.Response.WriteAsJsonAsync(new {Message = "Внутренняя ошибка сервера"});
        }
    }
}