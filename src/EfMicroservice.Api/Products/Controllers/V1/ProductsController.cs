using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Api.Products.Models;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.DeleteProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Queries.GetProductById;
using EfMicroservice.Application.Products.Queries.GetProducts;
using EfMicroservice.Common.ExceptionHandling.Exceptions;
using EfMicroservice.ExternalData.Clients.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Products.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IGetProductsQuery _getProductsQuery;
        private readonly IGetProductByIdQuery _getProductByIdQuery;
        private readonly ICreateProductCommand _createProductCommand;
        private readonly IUpdateProductCommand _updateProductCommand;
        private readonly IDeleteProductCommand _deleteProductCommand;
        private readonly IGitHaubClient _haubService;
        private readonly ILogger _logger;

        public ProductsController(IGetProductsQuery getProductsQuery, IGetProductByIdQuery getProductByIdQuery, ICreateProductCommand createProductCommand, IUpdateProductCommand updateProductCommand, IDeleteProductCommand deleteProductCommand, ILoggerFactory loggerFactory, IGitHaubClient haubService)
        {
            _getProductsQuery = getProductsQuery;
            _getProductByIdQuery = getProductByIdQuery;
            _createProductCommand = createProductCommand;
            _updateProductCommand = updateProductCommand;
            _deleteProductCommand = deleteProductCommand;
            _haubService = haubService;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var downstreamRequest = await _haubService.SendAsyncDoesPost();
            _logger.LogError("Logging Things!!!");

            // throw new BadRequestException("WRONG");

            return Ok(await _getProductsQuery.ExecuteAsync());
        }

        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            var product = await _getProductByIdQuery.ExecuteAsync(id);

            if (product == null)
            {
                throw new NotFoundException();
            }

            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public async Task<IActionResult> Post([FromBody] CreateProductRequest request)
        {
            var newProduct = new CreateProductModel()
            {
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity
            };

            var createdProduct = await _createProductCommand.ExecuteAsync(newProduct);

            return CreatedAtRoute("GetValueById", new {id = createdProduct.Id}, createdProduct);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateProductRequest request)
        {
            var updatedProduct = new UpdateProductModel()
            {
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity,
                RowVersion = request.RowVersion
            };

            await _updateProductCommand.ExecuteAsync(id, updatedProduct);

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _deleteProductCommand.ExecuteAsync(id);

            return NoContent();
        }
    }
}
