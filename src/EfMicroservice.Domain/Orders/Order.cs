using EfMicroservice.Domain.Orders.Exceptions;
using EfMicroservice.Domain.Products;
using FluentValidation;
using Omni.BuildingBlocks.Persistence;
using System;
using EfMicroservice.Common;

namespace EfMicroservice.Domain.Orders
{
    public class Order : BaseEntity<int>, IVersionInfo
    {
        private static readonly OrderValidator _validator = new OrderValidator();

        public Guid ProductId { get; protected set; }

        public Product Product { get; set; }

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

        public byte[] RowVersion { get; set; }

        //public OrderStatus Status { get; private set; }
        public OrderStatuses StatusId { get; set; }
        public OrderStatus Status { get; private set; }

        protected Order()
        {
            StatusId = OrderStatuses.Submitted;
        }

        public Order(Guid productId, int quantity)
            : this()
        {
            ProductId = productId;
            Quantity = quantity;
            _validator.ValidateAndThrow(this);
        }

        public void SetCancelledStatus()
        {
            if (StatusId == OrderStatuses.Shipped)
            {
                StatusChangeException(OrderStatuses.Cancelled);
            }

            StatusId = OrderStatuses.Cancelled;
        }

        private void StatusChangeException(OrderStatuses orderStatusesToChange)
        {
            throw new OrderingDomainException($"Is not possible to change the order status from {StatusId} to {orderStatusesToChange}.");
        }

        public decimal TotalCost()
        {
            return Quantity * Product.Price;
        }
    }
}