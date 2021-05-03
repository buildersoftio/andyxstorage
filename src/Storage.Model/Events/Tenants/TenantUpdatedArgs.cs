using Buildersoft.Andy.X.Storage.Model.Shared.Tenants;
using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantUpdatedArgs
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public Encryption Encryption { get; set; }
        public Signature Signature { get; set; }
    }
}
