using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

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
        public async Task<ActionResult<OrderModel>> Post([FromBody] PlaceOrderModel newProduct)
        {
            var createdOrder = await _placeOrderCommand.ExecuteAsync(newProduct);

            return Created(string.Empty, createdOrder);
        }
    }
}