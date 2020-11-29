using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Products
{
    public class ProductDeletedArgs
    {
        public string TenantName { get; set; }
        public string ProductName { get; set; }

        // TODO... Add properties for this Product
    }
}
