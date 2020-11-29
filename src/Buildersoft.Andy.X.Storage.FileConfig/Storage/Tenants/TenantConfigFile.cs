using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants
{
    public static class TenantConfigFile
    {
        public static ConcurrentDictionary<string, Tenant> GetTenants()
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(rootPath, "Storage", "Tenants", "tenants_config.json");

            if (!Directory.Exists(Path.Combine(rootPath, "Storage")))
                Directory.CreateDirectory(Path.Combine(rootPath, "Storage"));

            if (!Directory.Exists(Path.Combine(rootPath, "Storage", "Tenants")))
                Directory.CreateDirectory(Path.Combine(rootPath, "Storage", "Tenants"));

            if (!File.Exists(filePath))
                return new ConcurrentDictionary<string, Tenant>();

            return File.ReadAllText(filePath).JsonToObject<ConcurrentDictionary<string, Tenant>>();
        }

        public static bool UpdateTenants(ConcurrentDictionary<string, Tenant> tenants)
        {
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(rootPath, "Storage", "Tenants", "tenants_config.json");

            if (!Directory.Exists(Path.Combine(rootPath, "Storage")))
                Directory.CreateDirectory(Path.Combine(rootPath, "Storage"));

            if (!Directory.Exists(Path.Combine(rootPath, "Storage", "Tenants")))
                Directory.CreateDirectory(Path.Combine(rootPath, "Storage", "Tenants"));

            if (tenants != null)
                File.WriteAllText(filePath, tenants.ToPrettyJson());
            else
                return false;

            return true;
        }

        public static string CreateTenantLocation(string tenantName)
        {
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Tenants");
            string tenantLocation = Path.Combine(rootPath, tenantName);
            if (!Directory.Exists(tenantLocation))
            {
                Directory.CreateDirectory(tenantLocation);
                return tenantLocation;
            }
            return tenantLocation;
        }
        public static string CreateProductLocation(string tenantName, string productName)
        {
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Tenants", tenantName);
            string productLocation = Path.Combine(rootPath, productName);

            if (!Directory.Exists(rootPath))
            {
                // TODO...
                // This should update the tenant json somehow!
                Directory.CreateDirectory(rootPath);
            }

            if (!Directory.Exists(productLocation))
            {
                Directory.CreateDirectory(productLocation);
                return productLocation;
            }

            return productLocation;
        }
        public static string CreateComponentLocation(string tenantName, string productName, string componentName)
        {
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Tenants", tenantName, productName);
            string componentLocation = Path.Combine(rootPath, componentName);

            if (!Directory.Exists(rootPath))
            {
                // TODO...
                // This should update the tenant json somehow!
                Directory.CreateDirectory(rootPath);
            }

            if (!Directory.Exists(componentLocation))
            {
                Directory.CreateDirectory(componentLocation);
                return componentLocation;
            }
            return componentLocation;
        }
        public static string CreateBookLocation(string tenantName, string productName, string componentName, string bookName, string currentFragmentId = "000000")
        {
            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage", "Tenants", tenantName, productName, componentName);
            string bookLocation = Path.Combine(rootPath, bookName);

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            if (!Directory.Exists(bookLocation))
            {
                Directory.CreateDirectory(bookLocation);
                Directory.CreateDirectory(Path.Combine(bookLocation, $"Messages_{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, $"Keys_{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Readers"));

                return bookLocation;
            }

            if (!Directory.Exists(Path.Combine(bookLocation, $"Messages_{currentFragmentId}")))
            {
                Directory.CreateDirectory(Path.Combine(bookLocation, $"Messages_{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, $"Keys_{currentFragmentId}"));
            }

            return bookLocation;
        }
        public static void RenameStorageDirectory(string location, string suffix)
        {
            Directory.Move(location, $"{location}_{suffix}");
        }
    }
}
