using Buildersoft.Andy.X.Storage.Model.Shared.Components;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.Shared.Products
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public ConcurrentDictionary<string, Component> Components { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Location { get; set; }

        public Product()
        {
            Components = new ConcurrentDictionary<string, Component>();
        }
    }
}
