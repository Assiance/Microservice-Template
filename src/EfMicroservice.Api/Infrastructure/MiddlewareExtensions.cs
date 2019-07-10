using EfMicroservice.Api.Infrastructure.Exceptions;
using EfMicroservice.Api.Infrastructure.Logging;
using EfMicroservice.Api.Infrastructure.MiddleWare;
using Microsoft.AspNetCore.Builder;

namespace EfMicroservice.Api.Infrastructure
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }

        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        public static IApplicationBuilder UseCorrelationIdInHeaderMiddleware(this IApplicationBuilder builer)
        {
            return builer.UseMiddleware<AddCorrelationIdToHeaderMiddleware>();
        }
    }
}
