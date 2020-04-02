using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MediatR;

namespace EfMicroservice.Api.Orders.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrdersController(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<OrdersController>();
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderModel), 201)]
        public async Task<ActionResult<OrderModel>> Post([FromBody] PlaceOrderCommand newProduct)
        {
            var createdOrder = await _mediator.Send(newProduct);

            return Created(string.Empty, createdOrder);
        }
    }
}