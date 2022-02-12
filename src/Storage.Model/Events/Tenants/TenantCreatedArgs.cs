using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using System;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Tenants
{
    public class TenantCreatedArgs
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public TenantSettings Settings { get; set; }

        public List<string> StoragesAlreadySent { get; set; }

        public TenantCreatedArgs()
        {
            Settings = new TenantSettings();
            StoragesAlreadySent = new List<string>();
        }
    }
}
