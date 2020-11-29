using Buildersoft.Andy.X.Storage.Data.Model.Events.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Products;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface IProductService
    {
        void CreateProduct(ProductCreatedArgs productCreatedArgs);
        Product UpdateProduct(ProductUpdatedArgs productDeletedArgs);
        Product ReadProduct(ProductReadArgs productReadArgs);
        ConcurrentDictionary<string, Product> ReadProducts();
        bool DeleteProduct(ProductDeletedArgs productDeletedArgs);
    }
}
