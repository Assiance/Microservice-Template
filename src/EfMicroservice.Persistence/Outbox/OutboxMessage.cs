using System;

namespace EfMicroservice.Persistence.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public DateTimeOffset? ProcessedAt { get; set; }
        public string Error { get; set; }
        public int RetryCount { get; set; }
    }
}
