using System;
using System.Collections.Generic;
using System.Text;
using EfMicroservice.Function.Api.Infrastructure.Exceptions;
using EfMicroservice.Function.Api.Infrastructure.Logging;
using Serverless.Function.Middleware.Abstractions;

namespace EfMicroservice.Function.Api.Shared
{
    public abstract class BaseController : FunctionBase
    {
        protected readonly IFunctionApplicationBuilder _builder;

        protected BaseController(IFunctionApplicationBuilder builder) : base(builder.ApplicationServices)
        {
            _builder = builder;

            _builder.UseMiddleware<LoggingMiddleware>();
            _builder.UseMiddleware<AddCorrelationIdToHeaderMiddleware>();
            _builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
