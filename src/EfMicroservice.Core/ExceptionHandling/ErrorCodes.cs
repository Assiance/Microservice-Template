using System.ComponentModel;
using System.Net;

namespace EfMicroservice.Core.ExceptionHandling
{
    public enum ErrorCode
    {
        [ErrorHttpCode(HttpStatusCode.InternalServerError)]
        [Description("This error code represents any unhandled exception.")]
        System,

        [ErrorHttpCode(HttpStatusCode.BadRequest)]
        [Description("This error code is returned when argument passed is invalid. Details may return the argumentName that is invalid if set.")]
        InvalidArgumentException,

        [ErrorHttpCode(HttpStatusCode.BadRequest)]
        [Description("This error code is returned when data passed fails modelstate validation. Details will return the errors with property information.")]
        ValidationException,

        [ErrorHttpCode(HttpStatusCode.Conflict)]
        [Description("This error code is returned when two users are updating an entity at the same time.")]
        ConcurrencyException,

        [ErrorHttpCode(HttpStatusCode.Conflict)]
        [Description("This error code is returned when there is a duplicate key. Details may return the key that is found to be duplicate if set.")]
        DuplicateKeyException,

        [ErrorHttpCode(HttpStatusCode.Forbidden)]
        [Description("This error code is returned when there is an authorization error.")]
        AccessDenied,

        [ErrorHttpCode(HttpStatusCode.NotFound)]
        [Description("This error code is returned when entity being updated is not found.")]
        KeyNotFoundException,

        [ErrorHttpCode(HttpStatusCode.BadRequest)]
        [Description("This error code is returned when request is not valid.")]
        BadRequest
    }
}
