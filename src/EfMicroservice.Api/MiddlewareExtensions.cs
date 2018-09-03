using EfMicroservice.Api.Logging;
using Microsoft.AspNetCore.Builder;

namespace EfMicroservice.Api
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }
    }
}
