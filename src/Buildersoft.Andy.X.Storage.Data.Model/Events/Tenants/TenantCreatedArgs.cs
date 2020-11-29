using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Tenants
{
    public class TenantCreatedArgs
    {
        public Guid TenantId { get; set; }
        public string TenantName { get; set; }
        public Encryption Encryption { get; set; }
        public Signature Signature { get; set; }
    }
}
