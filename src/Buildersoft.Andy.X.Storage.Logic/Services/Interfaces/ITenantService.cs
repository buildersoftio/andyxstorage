using Buildersoft.Andy.X.Storage.Data.Model.Events.Tenants;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface ITenantService
    {
        void CreateTenant(TenantCreatedArgs tenantCreatedArgs);
        Tenant UpdateTenant(TenantUpdatedArgs tenantDeletedArgs);
        Tenant ReadTenant(TenantReadArgs tenantReadArgs);
        ConcurrentDictionary<string, Tenant> ReadTenants();
        bool DeleteTenant(TenantDeletedArgs tenantDeletedArgs);
    }
}
