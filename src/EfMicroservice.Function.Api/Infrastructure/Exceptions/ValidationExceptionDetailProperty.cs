namespace EfMicroservice.Function.Api.Infrastructure.Exceptions
{
    public class ValidationExceptionDetailProperty
    {
        public string PropertyName { get; set; }

        public object PropertyValue { get; set; }

        public string ErrorMessage { get; set; }
    }
}