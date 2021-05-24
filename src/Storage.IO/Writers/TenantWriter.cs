﻿using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Writers
{
    public static class TenantWriter
    {
        public static bool WriteTenantConfigFromFile(Tenant tenant)
        {
            try
            {
                if (File.Exists(TenantLocations.GetTenantConfigFile(tenant.Name)))
                    File.Delete(TenantLocations.GetTenantConfigFile(tenant.Name));

                File.WriteAllText(TenantLocations.GetTenantConfigFile(tenant.Name), tenant.ToJsonAndEncrypt());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async static void WriteInTenantLog(string tenantName, string rowLog)
        {
            try
            {
                await File.AppendAllTextAsync(TenantLocations.GetTenantTodayLogFile(tenantName), $"{rowLog}\n");
            }
            catch (Exception)
            {
                // TODO: handle this exception
            }
        }
    }
}