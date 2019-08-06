using System;
using System.Linq;
using System.Net;

namespace EfMicroservice.Common.ExceptionHandling.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException()
            : this(ErrorCode.System)
        {
        }

        public BaseException(string message)
            : this(code: ErrorCode.System, details: null, inner: null, httpCode: null, message: message)
        {
        }

        public BaseException(Exception ex)
            : this(code: ErrorCode.System, inner: ex)
        {
        }

        public BaseException(string message, ErrorCode code)
            : this(code: code, details: null, inner: null, httpCode: null, message: message)
        {
        }

        public BaseException(ErrorCode code, dynamic details = null, Exception inner = null,
            HttpStatusCode? httpCode = null, string message = null)
            : base(message, inner)
        {
            ErrorCode = code;
            HttpCode = httpCode ?? ExtractHttpErrorCode(code);
            Details = details;
        }

        public ErrorCode ErrorCode { get; set; }

        public HttpStatusCode HttpCode { get; set; }

        public dynamic Details { get; set; }

        public static HttpStatusCode ExtractHttpErrorCode(ErrorCode code)
        {
            var httpCode = HttpStatusCode.InternalServerError;

            var type = code.GetType();
            var memInfo = type.GetMember(code.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(ErrorHttpCodeAttribute), false);
            var firstAttribute = attributes.FirstOrDefault();

            if (firstAttribute is ErrorHttpCodeAttribute a)
            {
                httpCode = a.Code;
            }

            return httpCode;
        }

        public virtual dynamic GetDetails()
        {
            return Details;
        }
    }
}