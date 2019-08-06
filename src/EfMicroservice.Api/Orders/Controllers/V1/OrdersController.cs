using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfMicroservice.Api.Orders.Models;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Orders.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IPlaceOrderCommand _placeOrderCommand;
        private readonly ILogger _logger;

        public OrdersController(IPlaceOrderCommand placeOrderCommand, ILoggerFactory loggerFactory)
        {
            _placeOrderCommand = placeOrderCommand;
            _logger = loggerFactory.CreateLogger<OrdersController>();
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderModel), 201)]
        public async Task<ActionResult<OrderModel>> Post([FromBody] CreateOrderRequest request)
        {
            var newProduct = new PlaceOrderModel()
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var createdOrder = await _placeOrderCommand.ExecuteAsync(newProduct);

            return CreatedAtRoute("GetValueById", new {id = createdOrder.Id}, createdOrder);
        }
    }
}