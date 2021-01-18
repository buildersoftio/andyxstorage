using Buildersoft.Andy.X.Storage.Data.Model.Events.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;

        private ProductRepository _productRepository;
        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public void CreateProduct(ProductCreatedArgs productCreatedArgs)
        {
            _productRepository = new ProductRepository(productCreatedArgs.TenantName);

            Product product;

            if (_productRepository.GetAll().ContainsKey(productCreatedArgs.ProductName) == true)
                throw new Exception($"There is a product registered with this name andyx://{productCreatedArgs.TenantName}/{productCreatedArgs.ProductName}");

            string productLocation = TenantConfigFile.CreateProductLocation(productCreatedArgs.TenantName, productCreatedArgs.ProductName);

            product = new Product()
            {
                Id = productCreatedArgs.ProductId,
                Name = productCreatedArgs.ProductName,
                Status = productCreatedArgs.ProductStatus,
                Description = productCreatedArgs.ProductDescription,
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Location = productLocation
            };
            _productRepository.Add(productCreatedArgs.ProductName, product);
            _logger.LogInformation($"{productCreatedArgs.TenantName}/{productCreatedArgs.ProductName}: stored");
        }

        public bool DeleteProduct(ProductDeletedArgs ProductDeletedArgs)
        {
            throw new NotImplementedException();
        }

        public Product ReadProduct(ProductReadArgs productReadArgs)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, Product> ReadProducts()
        {
            throw new NotImplementedException();
        }

        public Product UpdateProduct(ProductUpdatedArgs productDeletedArgs)
        {
            throw new NotImplementedException();
        }
    }
}
