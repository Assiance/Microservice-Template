using Microsoft.AspNetCore.Http;

namespace EfMicroservice.Core.Http.CorrelationId
{
    public interface ICorrelationIdProvider
    {
        string EnsureCorrelationIdPresent(HttpRequest request);
    }
}