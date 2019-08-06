using System;
using System.Threading.Tasks;
using EfMicroservice.Application.Orders.Queries;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public interface IPlaceOrderCommand
    {
        Task<OrderModel> ExecuteAsync(PlaceOrderModel orderToCreate);
    }
}