using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{

    public class ProductEventHandler
    {
        private readonly NodeDataStorageService _service;
        private readonly IProductService _productService;

        public ProductEventHandler(NodeDataStorageService service, IProductService productService)
        {
            _service = service;
            _productService = productService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.ProductCreated += DataStorageService_ProductCreated;
            _service.ProductRead += DataStorageService_ProductRead;
            _service.ProductUpdated += DataStorageService_ProductUpdated;
            _service.ProductDeleted += DataStorageService_ProductDeleted;
        }

        private void DataStorageService_ProductDeleted(Data.Model.Events.Products.ProductDeletedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ProductUpdated(Data.Model.Events.Products.ProductUpdatedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ProductRead(Data.Model.Events.Products.ProductReadArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ProductCreated(Data.Model.Events.Products.ProductCreatedArgs obj)
        {
            _productService.CreateProduct(obj);
        }
    }
}
