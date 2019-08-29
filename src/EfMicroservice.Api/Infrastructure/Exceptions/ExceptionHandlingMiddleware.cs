using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using EfMicroservice.Common.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EfMicroservice.Api.Infrastructure.Exceptions
{
    public class ExceptionHandlingMiddleware
    {
        private readonly ILogger _logger;
        private readonly IErrorResultConverter _errorResultConverter;
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory,
            IErrorResultConverter errorResultConverter)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory.CreateLogger<ExceptionHandlingMiddleware>();
            _errorResultConverter = errorResultConverter;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            try
            {
                await _next(httpContext);
            }
            catch (BaseException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int) ex.HttpCode, errorResult);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int) HttpStatusCode.BadRequest, errorResult);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int) HttpStatusCode.InternalServerError, errorResult);
            }
            catch (HttpCallException exception)
            {
                var errorResult = _errorResultConverter.GetError(exception);
                await WriteErrorAsync(httpContext, exception, (int) exception.StatusCode, errorResult);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int)HttpStatusCode.Conflict, errorResult);
            }
            catch (Exception ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int) HttpStatusCode.InternalServerError, errorResult);
            }
        }

        private Task WriteErrorAsync(HttpContext context, Exception exception, int httpStatusCode,
            ErrorResult errorResult, LogLevel logLevel = LogLevel.Error)
        {
            context.Response.StatusCode = httpStatusCode;
            var payloadContent = JsonConvert.SerializeObject(errorResult, JsonSettings);

            _logger.Log(logLevel, new EventId(context.Response.StatusCode), exception, payloadContent);

            context.Response.ContentType = "application/json";
            return context.Response.WriteAsync(payloadContent);
        }

        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
            {ContractResolver = new CamelCasePropertyNamesContractResolver()};
    }
}