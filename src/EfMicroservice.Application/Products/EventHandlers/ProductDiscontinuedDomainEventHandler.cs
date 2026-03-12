using EfMicroservice.Application.Products.IntegrationEvents;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.EventHandlers
{
    public class ProductDiscontinuedDomainEventHandler : INotificationHandler<ProductDiscontinuedDomainEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductDiscontinuedDomainEventHandler> _logger;

        public ProductDiscontinuedDomainEventHandler(IUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger<ProductDiscontinuedDomainEventHandler>();
        }

        public async Task Handle(ProductDiscontinuedDomainEvent productDiscontinuedDomainEvent, CancellationToken cancellationToken)
        {
            var ordersToUpdate = await _unitOfWork.Orders.FindUnShippedOrdersByProductIdAsync(productDiscontinuedDomainEvent.Product.Id);
            foreach (var order in ordersToUpdate)
            {
                order.SetCancelledStatus();
            }

            productDiscontinuedDomainEvent.Product.AddIntegrationEvent(
                new ProductDiscontinuedIntegrationEvent
                {
                    ProductId = productDiscontinuedDomainEvent.Product.Id,
                    ProductName = productDiscontinuedDomainEvent.Product.Name
                });

            _logger.LogTrace("Product with Id: {ProductId} has been discontinued", productDiscontinuedDomainEvent.Product.Id);
        }
    }
}
