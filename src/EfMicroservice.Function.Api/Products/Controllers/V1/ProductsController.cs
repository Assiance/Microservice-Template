using EfMicroservice.Application.Products.Clients;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.DeleteProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Application.Products.Queries.GetProductById;
using EfMicroservice.Application.Products.Queries.GetProducts;
using EfMicroservice.Function.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Api.Extensions;
using Serverless.Function.Middleware.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Commands.Discontinue;

namespace EfMicroservice.Function.Api.Products.Controllers.V1
{
    public class ProductsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IProductMapper _mapper;
        private readonly IGitHaubClient _haubClient;
        private readonly ILogger _logger;

        public ProductsController(IFunctionApplicationBuilder builder, IMediator mediator, IProductMapper mapper, IGitHaubClient haubClient, ILoggerFactory loggerFactory) : base(builder)
        {
            _mediator = mediator;
            _mapper = mapper;
            _haubClient = haubClient;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [FunctionName(nameof(GetProducts))]
        [ProducesResponseType(typeof(IEnumerable<ProductModel>), 200)]
        public async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products")]
            HttpRequest req,
            ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                //var downstreamRequest = await _haubClient.Get();
                //var downstreamRequest2 = await _haubService.SendAsyncDoesPost();

                return new OkObjectResult(await _mediator.Send(new GetProductsQuery()));
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(GetProductById))]
        [ProducesResponseType(typeof(ProductModel), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id, ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                return new OkObjectResult(await _mediator.Send(new GetProductByIdQuery(id)));
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(CreateProduct))]
        [ProducesResponseType(typeof(ProductModel), 201)]
        public async Task<IActionResult> CreateProduct([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")]
            HttpRequest req,
            ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                var newProduct = await GetRequestBodyAndValidateAsync<CreateProductCommand>(req);
                var createdProduct = await _mediator.Send(newProduct);

                return new CreatedResult($"{req.Host}{req.Path}/{createdProduct.Id}", createdProduct);
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(UpdateProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct([HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id, ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                var updatedProduct = await GetRequestBodyAndValidateAsync<UpdateProductCommand>(req);

                updatedProduct.ProductId = id;
                await _mediator.Send(updatedProduct);

                return new NoContentResult();
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(DiscontinueProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DiscontinueProduct([HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}/discontinue")]
            HttpRequest req,
            Guid id, ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                var discontinuedProduct = await GetRequestBodyAndValidateAsync<DiscontinueProductCommand>(req);

                discontinuedProduct.ProductId = id;
                await _mediator.Send(discontinuedProduct);

                return new NoContentResult();
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(PatchProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PatchProduct([HttpTrigger(AuthorizationLevel.Function, "patch", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id, ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                var patch = await GetJsonBodyAsync<JsonPatchDocument<UpdateProductCommand>>(req);

                var supportedOps = new[] { OperationType.Replace };
                patch.IncludedPatchOps(supportedOps);

                var restrictedPaths = Array.Empty<string>();
                patch.ExcludedPatchPaths(restrictedPaths);

                var productModel = await _mediator.Send(new GetProductByIdQuery(id));
                var patchModel = _mapper.Map(productModel);

                patch.ApplyTo(patchModel);
                await _mediator.Send(patchModel);

                return new NoContentResult();
            });

            return await pipeline.RunAsync(req.HttpContext);
        }

        [FunctionName(nameof(DeleteProduct))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteProduct([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id, ILogger log)
        {
            var pipeline = _builder.UseFunction(async () =>
            {
                await _mediator.Send(new DeleteProductCommand()
                {
                    ProductId = id
                });

                return new NoContentResult();
            });

            return await pipeline.RunAsync(req.HttpContext);
        }
    }
}