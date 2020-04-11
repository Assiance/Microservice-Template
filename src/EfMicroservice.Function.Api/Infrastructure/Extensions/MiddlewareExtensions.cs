using EfMicroservice.Function.Api.Infrastructure.Exceptions;
using EfMicroservice.Function.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;

namespace EfMicroservice.Function.Api.Infrastructure.Extensions
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

        public static IApplicationBuilder UseAddCorrelationIdToHeaderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AddCorrelationIdToHeaderMiddleware>();
        }
    }
}