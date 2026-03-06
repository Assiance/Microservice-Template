using EfMicroservice.Application.Orders.Repositories;
using EfMicroservice.Application.Products.Repositories;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Persistence.Contexts;
using EfMicroservice.Persistence.Extensions;
using EfMicroservice.Persistence.Orders;
using EfMicroservice.Persistence.Outbox;
using EfMicroservice.Persistence.Products;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Application.Events;
using Omni.BuildingBlocks.Persistence;
using Omni.BuildingBlocks.Persistence.Extensions;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EfMicroservice.Persistence.Shared
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IChangeTrackingService _changeTrackingService;
        private readonly IMediator _mediator;
        private readonly ILoggerFactory _loggerFactory;

        private IProductRepository _productRepository;
        private IOrderRepository _orderRepository;

        public UnitOfWork(ApplicationDbContext dbContext, IChangeTrackingService changeTrackingService, IMediator mediator, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _changeTrackingService = changeTrackingService;
            _mediator = mediator;
            _loggerFactory = loggerFactory;
        }

        public IProductRepository Products
        {
            get { return _productRepository = _productRepository ?? new ProductRepository(_dbContext, _loggerFactory); }
        }

        public IOrderRepository Orders
        {
            get { return _orderRepository = _orderRepository ?? new OrderRepository(_dbContext, _loggerFactory); }
        }

        public async Task SaveAsync()
        {
            await OnBeforeSaveChangesAsync();
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public void UpdateRowVersion(IVersionInfo versionInfo, byte[] newRowVersion)
        {
            _dbContext.UpdateRowVersion(versionInfo, newRowVersion);
        }

        private async Task OnBeforeSaveChangesAsync()
        {
            // 1. Dispatch domain events (in-process MediatR) for same-BC side effects
            await _mediator.DispatchDomainEventsAsync(_dbContext);

            // 2. Serialize integration events from entities that produce outbox events
            var outboxEntities = _dbContext.ChangeTracker
                .Entries<IHasIntegrationEvents>()
                .Where(e => e.Entity.IntegrationEvents != null && e.Entity.IntegrationEvents.Any())
                .ToList();

            foreach (var entry in outboxEntities)
            {
                foreach (var integrationEvent in entry.Entity.IntegrationEvents)
                {
                    _dbContext.OutboxMessages.Add(new OutboxMessage
                    {
                        Id = Guid.NewGuid(),
                        Type = integrationEvent.GetType().FullName,
                        Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
                        OccurredAt = integrationEvent.OccurredAt
                    });
                }
                entry.Entity.ClearIntegrationEvents();
            }

            // 3. Apply audit tracking
            var entries = _dbContext.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                await _changeTrackingService.ExecuteResolversAsync(entry);
            }

            // 4. SaveChangesAsync — all in one transaction
        }
    }
}
