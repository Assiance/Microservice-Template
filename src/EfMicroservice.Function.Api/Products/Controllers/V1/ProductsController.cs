using EfMicroservice.Application.Products.Clients;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.DeleteProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Api.Extensions;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Queries.GetProductById;
using EfMicroservice.Application.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace EfMicroservice.Function.Api.Products.Controllers.V1
{
    public class ProductsController
    {
        private readonly IMediator _mediator;
        private readonly IProductMapper _mapper;
        private readonly IGitHaubClient _haubClient;
        private readonly ILogger _logger;

        public ProductsController(IMediator mediator, IProductMapper mapper, IGitHaubClient haubClient, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _mapper = mapper;
            _haubClient = haubClient;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [FunctionName(nameof(GetProducts))]
        [ProducesResponseType(typeof(IEnumerable<ProductModel>), 200)]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products")]
            HttpRequest req,
            ILogger log)
        {
            //var downstreamRequest = await _haubClient.Get();
            //var downstreamRequest2 = await _haubService.SendAsyncDoesPost();

            return new OkObjectResult(await _mediator.Send(new GetProductsQuery()));
        }

        [HttpGet("{id}", Name = nameof(GetProductById))]
        [FunctionName(nameof(GetProductById))]
        [ProducesResponseType(typeof(ProductModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductModel>> GetProductById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id}")]
            HttpRequest req,
            ILogger log, Guid id)
        {
            return new OkObjectResult(await _mediator.Send(new GetProductByIdQuery(id)));
        }

        //[HttpPost]
        //[ProducesResponseType(typeof(ProductModel), 201)]
        //public async Task<ActionResult<ProductModel>> Post([FromBody] CreateProductCommand newProduct)
        //{
        //    var createdProduct = await _mediator.Send(newProduct);

        //    return CreatedAtRoute(nameof(GetProductById), new { id = createdProduct.Id, version = "1" }, createdProduct);
        //}

        //[HttpPut("{id}")]
        //[ProducesResponseType(204)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> Put(Guid id, [FromBody] UpdateProductCommand updatedProduct)
        //{
        //    updatedProduct.ProductId = id;
        //    await _mediator.Send(updatedProduct);

        //    return NoContent();
        //}

        //[HttpPatch("{id}")]
        //[ProducesResponseType(204)]
        //[ProducesResponseType(404)]
        //public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateProductCommand> patch)
        //{
        //    var supportedOps = new[] { OperationType.Replace };
        //    patch.IncludedPatchOps(supportedOps);

        //    var restrictedPaths = Array.Empty<string>();
        //    patch.ExcludedPatchPaths(restrictedPaths);

        //    var productModel = await _mediator.Send(new GetProductByIdQuery(id));
        //    var patchModel = _mapper.Map(productModel);

        //    patch.ApplyTo(patchModel, ModelState);

        //    if (!ModelState.IsValid || !TryValidateModel(patchModel))
        //    {
        //        throw new BadRequestException(ModelState.ToFormattedErrors().Join());
        //    }

        //    await _mediator.Send(patchModel);

        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //[ProducesResponseType(204)]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    await _mediator.Send(new DeleteProductCommand()
        //    {
        //        ProductId = id
        //    });

        //    return NoContent();
        //}
    }
}