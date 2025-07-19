namespace VC.AG.WebAPI.Middlewares
{
    public class CustomSecurityHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomSecurityHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Append("X-Frame-Options", "DENY");

            context.Response.Headers.Append("X-Xss-Protection", "1; mode=block");

            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            context.Response.Headers.Append("Referrer-Policy", "no-referrer");

            context.Response.Headers.Append("Permissions-Policy", "camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), usb=()");

            await _next.Invoke(context);
        }
    }
}
