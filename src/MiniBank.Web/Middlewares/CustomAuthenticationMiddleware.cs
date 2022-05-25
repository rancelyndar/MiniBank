using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Net.Http.Headers;

namespace MiniBank.Web.Middlewares;

public class CustomAuthenticationMiddleware
{
    public readonly RequestDelegate next;

    public CustomAuthenticationMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        string token = context.Request.Headers[HeaderNames.Authorization];
        if (token == null)
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        token = token.ToString().Replace("Bearer ", "").Split('.')[1];
        string decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        JsonNode obj = JsonObject.Parse(decodedToken);
        double exp = (double) obj["exp"];
        DateTime expDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        expDateTime = expDateTime.AddSeconds(exp).ToLocalTime();

        if (expDateTime < DateTime.Now)
        {
            context.Response.StatusCode = 403;
            return;
        }
        await next(context);
    }
}