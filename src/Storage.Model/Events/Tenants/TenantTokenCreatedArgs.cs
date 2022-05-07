using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantTokenCreatedArgs
    {
        public string Tenant { get; set; }
        public TenantToken Token { get; set; }

        public List<string> StoragesAlreadySent { get; set; }

        public TenantTokenCreatedArgs()
        {
            Token = new TenantToken();
            StoragesAlreadySent = new List<string>();
        }
    }
}
