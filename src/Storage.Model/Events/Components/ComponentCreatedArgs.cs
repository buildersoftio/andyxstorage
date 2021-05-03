using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Components
{
    public class ComponentCreatedArgs
    {
        public Guid ComponentId { get; set; }
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }
        public bool ComponentStatus { get; set; }
    }
}
