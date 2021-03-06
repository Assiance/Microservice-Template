﻿using System;
using System.Collections.Generic;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Domain.Products;

namespace EfMicroservice.Application.Products.Mappings
{
    public interface IProductMapper
    {
        ProductModel Map(Product source);
        IList<ProductModel> Map(IList<Product> source);
        Product Map(CreateProductCommand source);
        Product Map(UpdateProductCommand source);
        UpdateProductCommand Map(ProductModel source);
    }
}