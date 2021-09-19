namespace vault
{
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    public class ShieldMiddleware
    {
        private readonly RequestDelegate _next;
        public ShieldMiddleware(RequestDelegate next, ILoggerFactory loggerFactory) 
            => this._next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.Headers["Server"] = "Yori/1.64.226 (cluster)";
            
            if (!context.Request.Headers.ContainsKey("User-Agent"))
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("");
                return;
            }
            if ($"{context.Request.Path}".EndsWith("php"))
            {
                context.Response.StatusCode = 451;
                await context.Response.WriteAsync("fuck u");
                return;
            }
            if ($"{context.Request.Path}".Contains("robots.txt"))
            {
                context.Response.StatusCode = 200;
                var robots = new StringBuilder();

                robots.AppendLine("User-agent: *");
                robots.AppendLine("Disallow: /");

                await context.Response.WriteAsync(robots.ToString());
                return;
            }
            await _next(context);
        }
    }
}