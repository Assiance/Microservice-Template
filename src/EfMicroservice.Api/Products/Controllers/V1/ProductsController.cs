using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.DeleteProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Application.Products.Queries.GetProductById;
using EfMicroservice.Application.Products.Queries.GetProducts;
using EfMicroservice.Common.Api.Extensions;
using EfMicroservice.Common.ExceptionHandling.Exceptions;
using EfMicroservice.ExternalData.Clients.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        private readonly IProductMapper _mapper;
        private readonly IGitHaubClient _haubService;
        private readonly ILogger _logger;

        public ProductsController(IGetProductsQuery getProductsQuery, IGetProductByIdQuery getProductByIdQuery,
            ICreateProductCommand createProductCommand, IUpdateProductCommand updateProductCommand,
            IDeleteProductCommand deleteProductCommand, IProductMapper mapper, ILoggerFactory loggerFactory, IGitHaubClient haubService)
        {
            _getProductsQuery = getProductsQuery;
            _getProductByIdQuery = getProductByIdQuery;
            _createProductCommand = createProductCommand;
            _updateProductCommand = updateProductCommand;
            _deleteProductCommand = deleteProductCommand;
            _mapper = mapper;
            _haubService = haubService;
            _logger = loggerFactory.CreateLogger<ProductsController>();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductModel>), 200)]
        public async Task<ActionResult<IEnumerable<ProductModel>>> Get()
        {
            var downstreamRequest = await _haubService.SendAsyncDoesPost();

            return Ok(await _getProductsQuery.ExecuteAsync());
        }

        [HttpGet("{id}", Name = nameof(GetProductById))]
        [ProducesResponseType(typeof(ProductModel), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductModel>> GetProductById(Guid id)
        {
            return Ok(await _getProductByIdQuery.ExecuteAsync(id));
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), 201)]
        public async Task<ActionResult<ProductModel>> Post([FromBody] CreateProductModel newProduct)
        {
            var createdProduct = await _createProductCommand.ExecuteAsync(newProduct);

            return CreatedAtRoute(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Put(Guid id, [FromBody] UpdateProductModel updatedProduct)
        {
            await _updateProductCommand.ExecuteAsync(id, updatedProduct);

            return NoContent();
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateProductModel> patch)
        {
            var supportedOps = new[] { OperationType.Replace };
            patch.IncludedPatchOps(supportedOps);

            var restrictedPaths = Array.Empty<string>();
            patch.ExcludedPatchPaths(restrictedPaths);

            var productModel = await _getProductByIdQuery.ExecuteAsync(id);
            var patchModel = _mapper.Map(productModel);

            patch.ApplyTo(patchModel, ModelState);

            if (!ModelState.IsValid || !TryValidateModel(patchModel))
            {
                throw new BadRequestException(ModelState.ToFormattedErrors().Join());
            }

            await _updateProductCommand.ExecuteAsync(id, patchModel);

            return NoContent();
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