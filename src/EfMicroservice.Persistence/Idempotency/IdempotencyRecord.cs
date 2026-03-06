using System;

namespace EfMicroservice.Persistence.Idempotency
{
    public class IdempotencyRecord
    {
        public Guid Key { get; set; }
        public string RequestType { get; set; }
        public string Response { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
