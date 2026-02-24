using EfMicroservice.Application.Products.Clients;
using EfMicroservice.Application.Products.Commands;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Models;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Function.Api.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfMicroservice.Function.Api.Products.Controllers.V1
{
    public class ProductsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IProductMapper _mapper;
        private readonly IGitHaubClient _haubClient;
        private readonly ILogger _logger;

        public ProductsController(IServiceProvider serviceProvider, IMediator mediator, IProductMapper mapper, IGitHaubClient haubClient, ILoggerFactory loggerFactory) : base(serviceProvider)
        {
            _mediator = mediator;
            _mapper = mapper;
            _haubClient = haubClient;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [Function(nameof(GetProducts))]
        [ProducesResponseType(typeof(IEnumerable<ProductModel>), 200)]
        public async Task<IActionResult> GetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products")]
            HttpRequest req)
        {
            return new OkObjectResult(await _mediator.Send(new GetProductsQuery()));
        }

        [Function(nameof(GetProductById))]
        [ProducesResponseType(typeof(ProductModel), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id)
        {
            return new OkObjectResult(await _mediator.Send(new GetProductByIdQuery(id)));
        }

        [Function(nameof(CreateProduct))]
        [ProducesResponseType(typeof(ProductModel), 201)]
        public async Task<IActionResult> CreateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")]
            HttpRequest req)
        {
            var newProduct = await GetRequestBodyAndValidateAsync<CreateProductCommand>(req);
            var createdProduct = await _mediator.Send(newProduct);
            return new CreatedResult($"{req.Host}{req.Path}/{createdProduct.Id}", createdProduct);
        }

        [Function(nameof(UpdateProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id)
        {
            var updatedProduct = await GetRequestBodyAndValidateAsync<UpdateProductCommand>(req);
            updatedProduct.ProductId = id;
            await _mediator.Send(updatedProduct);
            return new NoContentResult();
        }

        [Function(nameof(DiscontinueProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DiscontinueProduct(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}/discontinue")]
            HttpRequest req,
            Guid id)
        {
            var discontinuedProduct = await GetRequestBodyAndValidateAsync<DiscontinueProductCommand>(req);
            discontinuedProduct.ProductId = id;
            await _mediator.Send(discontinuedProduct);
            return new NoContentResult();
        }

        [Function(nameof(PatchProduct))]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> PatchProduct(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id)
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
        }

        [Function(nameof(DeleteProduct))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> DeleteProduct(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id}")]
            HttpRequest req,
            Guid id)
        {
            await _mediator.Send(new DeleteProductCommand()
            {
                ProductId = id
            });
            return new NoContentResult();
        }
    }
}
