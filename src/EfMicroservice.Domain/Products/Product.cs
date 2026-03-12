using EfMicroservice.Domain.Products.Events;
using EfMicroservice.Domain.Orders;
using FluentValidation;
using Omni.BuildingBlocks.Application.Events;
using Omni.BuildingBlocks.Persistence;
using System;
using System.Collections.Generic;

namespace EfMicroservice.Domain.Products
{
    public class Product : BaseEntity<Guid>, IVersionInfo, IAuditInfo, ISoftDeletable, IHasIntegrationEvents
    {
        private static readonly ProductValidator _validator = new ProductValidator();
        private List<IIntegrationEvent> _integrationEvents;

        public IReadOnlyCollection<IIntegrationEvent> IntegrationEvents => _integrationEvents?.AsReadOnly();

        public void AddIntegrationEvent(IIntegrationEvent @event)
        {
            _integrationEvents ??= new List<IIntegrationEvent>();
            _integrationEvents.Add(@event);
        }

        public void ClearIntegrationEvents()
        {
            _integrationEvents?.Clear();
        }

        private string _name;
        public string Name
        {
            get => _name;

            set
            {
                _name = value;
                _validator.ValidatePropertyAndThrow(this, (x) => x.Name);
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;

            set
            {
                _price = value;
                _validator.ValidatePropertyAndThrow(this, (x) => x.Price);
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;

            set
            {
                _quantity = value;
                _validator.ValidatePropertyAndThrow(this, (x) => x.Quantity);
            }
        }

        public ProductStatuses StatusId { get; set; }
        public ProductStatus Status { get; private set; }

        public byte[] RowVersion { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        // ISoftDeletable
        public DateTimeOffset? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        public IEnumerable<Order> Orders { get; set; }

        protected Product(int quantity)
        {
            StatusId = quantity == 0 ? ProductStatuses.OutOfStock : ProductStatuses.InStock;
        }

        public Product(string name, decimal price, int quantity)
            : this(quantity)
        {
            Name = name;
            Price = price;
            Quantity = quantity;

            _validator.ValidateAndThrow(this);
        }

        public void SetDiscontinueStatus()
        {
            StatusId = ProductStatuses.Discontinued;
            AddDomainEvent(new ProductDiscontinuedDomainEvent(this));
        }
    }
}
