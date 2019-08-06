namespace EfMicroservice.Api.Infrastructure.Exceptions
{
    public class Error
    {
        public Error(string source, string code, string message, dynamic details = null)
        {
            Source = source;
            Code = code;
            Message = message;
            Details = details;
        }

        public string Source { get; }

        public string Code { get; }

        public string Message { get; }

        public dynamic Details { get; set; }
    }
}