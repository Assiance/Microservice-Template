namespace EfMicroservice.Core.ExceptionHandling.Exceptions
{
    public class BadRequestException : BaseException
    {
        public BadRequestException() : this(null)
        {
        }

        public BadRequestException(string message) : base(message, ErrorCode.BadRequest)
        {
        }
    }
}
