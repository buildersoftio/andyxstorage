using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Components
{
    public class ComponentCreatedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
