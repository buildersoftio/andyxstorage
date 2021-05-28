using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantCreatedArgs
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DigitalSignature { get; set; }
    }
}
