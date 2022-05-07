using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantTokenRevokedArgs
    {
        public string Tenant { get; set; }
        public string Token { get; set; }

        public List<string> StoragesAlreadySent { get; set; }


        public TenantTokenRevokedArgs()
        {
            StoragesAlreadySent = new List<string>();
        }
    }
}
