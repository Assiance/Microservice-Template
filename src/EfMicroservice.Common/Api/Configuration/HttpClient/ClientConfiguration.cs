namespace EfMicroservice.Common.Api.Configuration.HttpClient
{
    public class ClientConfiguration
    {
        public string Namespace { get; set; }

        public string BaseUrl { get; set; } 

        public string TokenEndpointUrl { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }
    }
}