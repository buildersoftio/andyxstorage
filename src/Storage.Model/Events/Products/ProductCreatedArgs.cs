using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Products
{
    public class ProductCreatedArgs
    {
        public string Tenant { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
