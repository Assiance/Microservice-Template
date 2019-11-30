namespace EfMicroservice.Common.ExceptionHandling.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException() : base(ErrorCode.DuplicateKeyException)
        {
        }

        public ConflictException(string message) : base(message, ErrorCode.DuplicateKeyException)
        {
        }
    }
}
