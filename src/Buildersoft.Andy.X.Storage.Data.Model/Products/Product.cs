using Buildersoft.Andy.X.Storage.Data.Model.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Products
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ConcurrentDictionary<string, Component> Components { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Location { get; set; }

        public Product()
        {
            Components = new ConcurrentDictionary<string, Component>();
        }
        public void SetId(Guid productId)
        {
            Id = productId;
            IsActive = true;
        }
    }
}
