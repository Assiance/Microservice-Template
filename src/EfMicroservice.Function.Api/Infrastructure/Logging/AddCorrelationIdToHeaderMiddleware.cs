﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Omni.BuildingBlocks.Http;
using Omni.BuildingBlocks.Http.CorrelationId;
using Serverless.Function.Middleware;
using Serverless.Function.Middleware.Abstractions;

namespace EfMicroservice.Function.Api.Infrastructure.Logging
{
    public class AddCorrelationIdToHeaderMiddleware : IFunctionMiddleware
    {
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public AddCorrelationIdToHeaderMiddleware(ICorrelationIdProvider correlationIdProvider)
        {
            _correlationIdProvider = correlationIdProvider;
        }

        public async Task InvokeAsync(HttpContext context, FunctionRequestDelegate next)
        {
            var correlation = _correlationIdProvider.EnsureCorrelationIdPresent();
            var request = context.Request;
            var response = context.Response;

            request.Headers.TryGetValue(KnownHttpHeaders.CorrelationId, out StringValues requestHeaderValue);
            if (requestHeaderValue.FirstOrDefault() == null)
            {
                request.Headers.Add(KnownHttpHeaders.CorrelationId, correlation);
            }

            response.Headers.TryGetValue(KnownHttpHeaders.CorrelationId, out StringValues responseHeaderValue);
            if (responseHeaderValue.FirstOrDefault() == null)
            {
                response.Headers.Add(KnownHttpHeaders.CorrelationId, correlation);
            }

            await next(context);
        }
    }
}