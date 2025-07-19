using System.Net;

namespace VC.AG.WebAPI.Extensions
{
    public static class HttpSecurityHeaderExtensions
    {

        public static void AddHttpSecurityHeaders(this IServiceCollection services)
        {
            services.AddAntiforgery(x =>
            {
                x.SuppressXFrameOptionsHeader = true;
            });
        }

    }
}
