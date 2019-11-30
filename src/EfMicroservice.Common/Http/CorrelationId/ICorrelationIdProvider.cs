namespace EfMicroservice.Common.Http.CorrelationId
{
    public interface ICorrelationIdProvider
    {
        string EnsureCorrelationIdPresent();
    }
}