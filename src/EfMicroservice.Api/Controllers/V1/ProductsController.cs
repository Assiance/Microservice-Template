using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Core.Data;
using EfMicroservice.Core.Data.Extensions;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using EfMicroservice.Data.Contexts;
using EfMicroservice.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ProductsController(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger(nameof(ProductsController));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            _logger.LogError("Logging Things!!!");

            throw new BadRequestException("WRONG");

            return Ok(await _dbContext.Products.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}", Name = "GetValueById")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<string>> Get(Guid id)
        {
            var product = await _dbContext.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public async Task<IActionResult> Post([FromBody] ProductEntity product)
        {
            var newProduct = new ProductEntity()
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity
            };

            var createdProduct = await _dbContext.Products.AddAsync(newProduct);
            await _dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetValueById", new {id = createdProduct.Entity.Id}, createdProduct.Entity);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(Guid id, [FromBody] ProductEntity updatedProduct)
        {
            var retrievedProduct = await _dbContext.Products.FindAsync(id);

            if (retrievedProduct == null)
            {
                return NotFound();
            }

            retrievedProduct.Name = updatedProduct.Name;
            retrievedProduct.Price = updatedProduct.Price;
            retrievedProduct.Quantity = updatedProduct.Quantity;

            _dbContext.UpdateRowVersion(retrievedProduct, updatedProduct.RowVersion);
            _dbContext.Products.Update(retrievedProduct);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var retrievedProduct = await _dbContext.Products.FindAsync(id);

            if (retrievedProduct == null)
            {
                return NotFound();
            }

            _dbContext.Products.Remove(retrievedProduct);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
