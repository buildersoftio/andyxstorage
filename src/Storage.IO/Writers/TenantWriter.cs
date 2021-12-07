using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Components;
using Buildersoft.Andy.X.Storage.Model.App.Products;
using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Writers
{
    public static class TenantWriter
    {
        public static bool WriteTenantConfigFile(Tenant tenant)
        {
            try
            {
                if (File.Exists(TenantLocations.GetTenantConfigFile(tenant.Name)))
                    File.Delete(TenantLocations.GetTenantConfigFile(tenant.Name));

                File.WriteAllText(TenantLocations.GetTenantConfigFile(tenant.Name), tenant.ToPrettyJson());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool WriteProductConfigFile(string tenant, Product product)
        {
            try
            {
                if (File.Exists(TenantLocations.GetProductConfigFile(tenant, product.Name)))
                    File.Delete(TenantLocations.GetProductConfigFile(tenant, product.Name));

                File.WriteAllText(TenantLocations.GetProductConfigFile(tenant, product.Name), product.ToPrettyJson());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool WriteComponentConfigFile(string tenant, string product, Component component)
        {
            try
            {
                if (File.Exists(TenantLocations.GetComponentConfigFile(tenant, product, component.Name)))
                    File.Delete(TenantLocations.GetComponentConfigFile(tenant, product, component.Name));

                File.WriteAllText(TenantLocations.GetComponentConfigFile(tenant, product, component.Name), component.ToPrettyJson());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool WriteTopicConfigFile(string tenant, string product, string component, Topic topic)
        {
            try
            {
                if (File.Exists(TenantLocations.GetTopicConfigFile(tenant, product, component, topic.Name)))
                    File.Delete(TenantLocations.GetTopicConfigFile(tenant, product, component, topic.Name));

                File.WriteAllText(TenantLocations.GetTopicConfigFile(tenant, product, component, topic.Name), topic.ToPrettyJson());
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
