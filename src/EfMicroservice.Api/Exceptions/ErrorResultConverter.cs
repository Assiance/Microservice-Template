using System;
using System.ComponentModel.DataAnnotations;
using EfMicroservice.Core.ExceptionHandling;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using Newtonsoft.Json.Linq;

namespace EfMicroservice.Api.Exceptions
{
    public class ErrorResultConverter : IErrorResultConverter
    {
        private const string DefaultInstance = "EfMicroservice"; //Todo: EF-Change
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

        public ErrorResult GetError(HttpCallException exception)
        {
            dynamic details = new
            {
                RequestUrl = exception.RequestUri.ToString(),
                exception.RequestMethod,
                ErrorMessage = exception.Message,
                ResponseBody = GetResponseBody(exception),
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), DefaultErrorMessage, details);
            return new ErrorResult(error);
        }

        public ErrorResult GetError(Exception exception)
        {
            dynamic details = new
            {
                ErrorMessage = exception.Message,
                StackTrace = exception.StackTrace
            };

            var error = new Error(DefaultInstance, ErrorCode.System.ToString(), exception.Message, details);
            return new ErrorResult(error);
        }

        private static object GetResponseBody(HttpCallException httpCallException)
        {
            try
            {
                return JObject.Parse(httpCallException.ResponseBody);
            }
            catch
            {
                return httpCallException.ResponseBody;
            }
        }
    }
}
