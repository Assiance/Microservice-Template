namespace EfMicroservice.Common.ExceptionHandling.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException() : base(ErrorCode.KeyNotFoundException)
        {
        }

        public NotFoundException(string message) : base(message, ErrorCode.KeyNotFoundException)
        {
        }
    }
}
