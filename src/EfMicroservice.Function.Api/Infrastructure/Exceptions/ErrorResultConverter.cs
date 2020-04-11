using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Omni.BuildingBlocks.ExceptionHandling;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using Polly.CircuitBreaker;

namespace EfMicroservice.Function.Api.Infrastructure.Exceptions
{
    public class ErrorResultConverter : IErrorResultConverter
    {
        private const string DefaultInstance = "CompositeX"; //Todo: EF-Change
        private const string DefaultErrorMessage = "Internal Server Error";

        public ErrorResult GetError(BaseException exception)
        {
            dynamic details = new
            {
                ExceptionDetails = exception.Details,
                ExceptionMessage = exception.Message,
                ErrorCode = exception.ErrorCode.ToString(),
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, exception.ErrorCode.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(ValidationException exception)
        {
            dynamic details = new
            {
                ErrorMessage = exception.Message,
                PropertyName = string.Join(",", exception.ValidationResult.MemberNames),
                PropertyValue = exception.Value,
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(FluentValidation.ValidationException exception)
        {
            var errors = new List<ValidationExceptionDetailProperty>();
            if (exception.Errors.Any())
            {
                errors.AddRange(exception.Errors.Select(item => new ValidationExceptionDetailProperty
                {
                    ErrorMessage = item.ErrorMessage,
                    PropertyValue = item.AttemptedValue,
                    PropertyName = item.PropertyName
                }));
            }

            dynamic details = new
            {
                Errors = errors,
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), "Request Model Validation Failed",
                details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(HttpCallException exception)
        {
            dynamic details = new
            {
                RequestUrl = exception.RequestUri.ToString(),
                RequestMethod = exception.RequestMethod.Method,
                ErrorMessage = exception.Message,
                ResponseBody = GetResponseBody(exception.ResponseBody),
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(BrokenCircuitException<HttpResponseMessage> exception)
        {
            var detailsErrorMessage = HttpCallException.BuildMessage(exception.Result.RequestMessage.RequestUri,
                exception.Result.RequestMessage.Method, exception.Result.StatusCode, exception.Result.ReasonPhrase);

            dynamic details = new
            {
                RequestUrl = exception.Result.RequestMessage.RequestUri.ToString(),
                RequestMethod = exception.Result.RequestMessage.Method.Method,
                ErrorMessage = detailsErrorMessage,
                ResponseBody = GetResponseBody(exception.Result.Content.ReadAsStringAsync().Result),
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(DbUpdateConcurrencyException exception)
        {
            dynamic details = new
            {
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                InnerExceptionMessage = exception.InnerException?.Message
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(DbUpdateException exception)
        {
            dynamic details = new
            {
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                InnerExceptionMessage = exception.InnerException?.Message
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.ExtractDetails(), details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(Exception exception)
        {
            dynamic details = new
            {
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace,
                InnerExceptionMessage = exception.InnerException?.Message
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        private static object GetResponseBody(string responseBody)
        {
            try
            {
                return JObject.Parse(responseBody);
            }
            catch
            {
                return responseBody;
            }
        }
    }
}