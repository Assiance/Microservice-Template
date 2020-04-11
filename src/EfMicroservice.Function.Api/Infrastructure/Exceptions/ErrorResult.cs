namespace EfMicroservice.Function.Api.Infrastructure.Exceptions
{
    public class ErrorResult
    {
        public ErrorResult(Error error)
        {
            Error = error;
        }

        public Error Error { get; }
    }
}