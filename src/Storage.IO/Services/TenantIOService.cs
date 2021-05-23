using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public static class TenantIOService
    {
        public static bool TryCreateTenantDirectory(string tenantName, Tenant tenantDetails)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetTenantDirectory(tenantName)) == true)
                {
                    // Update the file from the Server.
                    TenantWriter.WriteTenantConfigFromFile(tenantDetails);
                }

                Directory.CreateDirectory(TenantLocations.GetTenantDirectory(tenantName));
                Directory.CreateDirectory(TenantLocations.GetProductRootDirectory(tenantName));
                Directory.CreateDirectory(TenantLocations.GetTenantLogsRootDirectory(tenantName));

                TenantWriter.WriteTenantConfigFromFile(tenantDetails);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void WriteAgentStateInTenantLog(string tenantName, string agentId, string state)
        {
            string log = $"{DateTime.Now:HH:mm:ss}|ANDYX-STORAGE#AGENTS|{agentId}|{state}";
            TenantWriter.WriteInTenantLog(tenantName, log);
        }
    }
}
