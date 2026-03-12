using Omni.BuildingBlocks.Application.Events;
using System;

namespace EfMicroservice.Application.Products.IntegrationEvents
{
    public record ProductDiscontinuedIntegrationEvent : IIntegrationEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
        public string EventType => nameof(ProductDiscontinuedIntegrationEvent);

        public Guid ProductId { get; init; }
        public string ProductName { get; init; }
    }
}
