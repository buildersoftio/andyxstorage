namespace Buildersoft.Andy.X.Storage.Model.Events.Products
{
    public class ProductUpdatedArgs
    {
        public string TenantName { get; set; }
        public string ProductName { get; set; }

        // Only status can change
        public bool ProductStatus { get; set; }
    }
}
