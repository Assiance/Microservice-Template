using EfMicroservice.Application.Products.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Orders.Consumers
{
    public class OrderCancellationConsumer : IConsumer<ProductDiscontinuedIntegrationEvent>
    {
        private readonly ILogger<OrderCancellationConsumer> _logger;

        public OrderCancellationConsumer(ILogger<OrderCancellationConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductDiscontinuedIntegrationEvent> context)
        {
            var @event = context.Message;
            _logger.LogInformation(
                "Handling ProductDiscontinued integration event for product {ProductId} — cancelling affected orders",
                @event.ProductId);

            // TODO: Cancel orders containing the discontinued product
            await Task.CompletedTask;
        }
    }
}
