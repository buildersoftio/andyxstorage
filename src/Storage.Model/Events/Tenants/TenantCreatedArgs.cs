using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantCreatedArgs
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DigitalSignature { get; set; }

        public TenantSettings Settings { get; set; }

        public TenantCreatedArgs()
        {
            Settings = new TenantSettings();
        }
    }
}
