using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Products
{
    public class ProductCreatedArgs
    {
        public Guid ProductId { get; set; }
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public bool ProductStatus { get; set; }
    }
}
