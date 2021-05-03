using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Components
{
    public class ComponentUpdatedArgs
    {
        public Guid ComponentId { get; set; }
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }

        // Only status can change
        public bool ComponentStatus { get; set; }
    }
}
