using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Api.Models.Products;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using EfMicroservice.Data.Clients.Interfaces;
using EfMicroservice.Domain.Model.Product;
using EfMicroservice.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IGitHaubService _haubService;
        private readonly ILogger _logger;

        public ProductsController(IProductService productService, ILoggerFactory loggerFactory, IGitHaubService haubService)
        {
            _productService = productService;
            _haubService = haubService;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var downstreamRequest = await _haubService.Get();
            _logger.LogError("Logging Things!!!");

            // throw new BadRequestException("WRONG");

            return Ok(await _productService.GetProductsAsync());
        }

        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

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
            var newProduct = new Product()
            {
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity
            };

            var createdProduct = await _productService.CreateProductAsync(newProduct);

            return CreatedAtRoute("GetValueById", new {id = createdProduct.Id}, createdProduct);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateProductRequest request)
        {
            var updatedProduct = new Product()
            {
                Id = id,
                Name = request.Name,
                Price = request.Price,
                Quantity = request.Quantity,
                RowVersion = request.RowVersion
            };

            await _productService.UpdateProductAsync(updatedProduct);

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteProductAsync(id);

            return NoContent();
        }
    }
}
