using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Infrastructure.MiddleWare
{
    public class AddCorrelationIdToHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public AddCorrelationIdToHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {

                await _next(context);

        }


    }
}
