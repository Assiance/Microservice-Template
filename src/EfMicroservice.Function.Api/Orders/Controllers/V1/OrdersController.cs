using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Models;
using EfMicroservice.Function.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EfMicroservice.Function.Api.Orders.Controllers.V1
{
    public class OrdersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrdersController(IServiceProvider serviceProvider, IMediator mediator, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<OrdersController>();
        }

        [Function(nameof(CreateOrder))]
        [ProducesResponseType(typeof(OrderModel), 201)]
        public async Task<IActionResult> CreateOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/orders")]
            HttpRequest req)
        {
            var newOrder = await GetRequestBodyAndValidateAsync<PlaceOrderCommand>(req);
            var createdOrder = await _mediator.Send(newOrder);
            return new CreatedResult(string.Empty, createdOrder);
        }
    }
}
