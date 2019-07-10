namespace EfMicroservice.Core.Api.Configuration.Authentication
{
    public class JwtConfiguration
    {
        public string ValidIssuer { get; set; }

        public string ValidAudience { get; set; }

        public string IssuerSigningKey { get; set; }
    }
}
