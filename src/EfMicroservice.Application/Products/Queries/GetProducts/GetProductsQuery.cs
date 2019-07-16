using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Application.Products.Queries.GetProducts
{
    public class GetProductsQuery : IGetProductsQuery
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggerFactory _loggerFactory;

        public GetProductsQuery(IProductMapper productMapper, IUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
            _loggerFactory = loggerFactory;
        }

        public async Task<IList<ProductModel>> ExecuteAsync()
        {
            var products = await _unitOfWork.Products.Queryable.ToListAsync();
            return _productMapper.Map(products);
        }
    }
}
