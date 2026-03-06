namespace EfMicroservice.Persistence.Outbox
{
    public class OutboxOptions
    {
        public int PollingIntervalSeconds { get; set; } = 5;
        public int BatchSize { get; set; } = 20;
        public int MaxRetries { get; set; } = 5;
    }
}
