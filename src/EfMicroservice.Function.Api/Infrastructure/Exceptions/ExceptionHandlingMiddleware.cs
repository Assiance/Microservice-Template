﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Omni.BuildingBlocks.ExceptionHandling;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using Polly.CircuitBreaker;
using Serverless.Function.Middleware;
using Serverless.Function.Middleware.Abstractions;

namespace EfMicroservice.Function.Api.Infrastructure.Exceptions
{
    public class ExceptionHandlingMiddleware : IFunctionMiddleware
    {
        private readonly ILogger _logger;
        private readonly IErrorResultConverter _errorResultConverter;

        public ExceptionHandlingMiddleware(ILoggerFactory loggerFactory,
            IErrorResultConverter errorResultConverter)
        {
            _logger = loggerFactory.CreateLogger<ExceptionHandlingMiddleware>();
            _errorResultConverter = errorResultConverter;
        }

        public async Task InvokeAsync(HttpContext httpContext, FunctionRequestDelegate next)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            try
            {
                await next(httpContext);
            }
            catch (BaseException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int)ex.HttpCode, errorResult);
            }
            catch (ValidationException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int)HttpStatusCode.BadRequest, errorResult);
            }
            catch (FluentValidation.ValidationException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);

                var statusCode = ex.Data.Contains(ExceptionDataKeys.IsBadRequest) && (bool) ex.Data[ExceptionDataKeys.IsBadRequest]
                    ? (int) HttpStatusCode.BadRequest
                    : (int) HttpStatusCode.InternalServerError;

                var logLevel = ex.Data.Contains(ExceptionDataKeys.IsBadRequest) && (bool)ex.Data[ExceptionDataKeys.IsBadRequest]
                    ? LogLevel.Information
                    : LogLevel.Error;

                await WriteErrorAsync(httpContext, ex, statusCode, errorResult, logLevel);
            }
            catch (HttpCallException exception)
            {
                var errorResult = _errorResultConverter.GetError(exception);
                await WriteErrorAsync(httpContext, exception, (int)exception.StatusCode, errorResult);
            }
            catch (BrokenCircuitException<HttpResponseMessage> exception)
            {
                var errorResult = _errorResultConverter.GetError(exception);
                await WriteErrorAsync(httpContext, exception, (int)exception.Result.StatusCode, errorResult);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int)HttpStatusCode.Conflict, errorResult);
            }
            catch (DbUpdateException ex)
            {
                var details = ex.ExtractDetails();
                var errorResult = _errorResultConverter.GetError(ex);

                var statusCode = details.Contains("is not present in table")
                    ? (int)HttpStatusCode.NotFound
                    : (int)HttpStatusCode.Conflict;

                await WriteErrorAsync(httpContext, ex, statusCode, errorResult);
            }
            catch (Exception ex)
            {
                var errorResult = _errorResultConverter.GetError(ex);
                await WriteErrorAsync(httpContext, ex, (int)HttpStatusCode.InternalServerError, errorResult);
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
        { ContractResolver = new CamelCasePropertyNamesContractResolver() };
    }
}