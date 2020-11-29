using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Components
{
    public class ComponentDeletedArgs
    {
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }

        // TODO... Add properties for this component
    }
}
