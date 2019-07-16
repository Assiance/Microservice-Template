using EfMicroservice.Domain.Orders;

namespace EfMicroservice.Application.UnitTests.Builders
{
    public class OrderBuilder
    {
        private readonly Order _order;

        public OrderBuilder()
        {
            _order = new Order();
        }

        public OrderBuilder WithId(int id)
        {
            _order.Id = id;
            return this;
        }

        public OrderBuilder WithQuantity(int quantity)
        {
            _order.Quantity = quantity;
            return this;
        }

        public Order Build()
        {
            return _order;
        }
    }
}
