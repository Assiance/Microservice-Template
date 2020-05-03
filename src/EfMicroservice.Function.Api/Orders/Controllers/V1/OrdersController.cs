using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Orders.Queries;
using EfMicroservice.Function.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Serverless.Function.Middleware.Abstractions;
using System.Threading.Tasks;

namespace EfMicroservice.Function.Api.Orders.Controllers.V1
{
    public class OrdersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public OrdersController(IFunctionApplicationBuilder builder, IMediator mediator, ILoggerFactory loggerFactory) : base(builder)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger<OrdersController>();
        }

        [FunctionName(nameof(CreateOrder))]
        [ProducesResponseType(typeof(OrderModel), 201)]
        public async Task<IActionResult> CreateOrder([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/orders")]
            HttpRequest req,
            ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                var newOrder = await GetRequestBodyAndValidateAsync<PlaceOrderCommand>(req);
                var createdOrder = await _mediator.Send(newOrder);

                return new CreatedResult(string.Empty, createdOrder);
            });

            return await pipeline.RunAsync(req.HttpContext);
        }
    }
}
