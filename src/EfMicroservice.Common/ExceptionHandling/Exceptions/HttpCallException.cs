using System;
using System.Net;
using System.Net.Http;

namespace EfMicroservice.Common.ExceptionHandling.Exceptions
{
    public class HttpCallException : Exception
    {
        public Uri RequestUri { get; }

        public HttpMethod RequestMethod { get; }

        public HttpStatusCode StatusCode { get; }

        public string ReasonPhrase { get; }

        public string ResponseBody { get; }

        public HttpCallException(Uri requestUri, HttpMethod requestMethod, HttpStatusCode statusCode,
            string reasonPhrase, string responseBody)
            : base(BuildMessage(requestUri, requestMethod, statusCode, reasonPhrase))
        {
            RequestUri = requestUri;
            RequestMethod = requestMethod;
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseBody = responseBody;
            Data["ResponseBody"] = responseBody;
        }

        private static string BuildMessage(Uri requestUri, HttpMethod requestMethod, HttpStatusCode statusCode,
            string reasonPhrase)
        {
            return $"Request {requestMethod} {requestUri} failed with {(int) statusCode} {reasonPhrase}";
        }
    }
}