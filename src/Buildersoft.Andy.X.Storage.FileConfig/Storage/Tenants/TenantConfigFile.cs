using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Buildersoft.Andy.X.Storage.IO.Storage.Tenants
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

            var tenants = File.ReadAllText(filePath).JsonToObject<ConcurrentDictionary<string, Tenant>>();
            UpdateSchemaInTenantsList(tenants, rootPath);

            return tenants;
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
                Directory.CreateDirectory(Path.Combine(bookLocation, "Messages"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Messages", $"{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Keys"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Keys", $"{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Readers"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Schemas"));

                return bookLocation;
            }

            if (!Directory.Exists(Path.Combine(bookLocation, "Messages", $"{currentFragmentId}")))
            {
                Directory.CreateDirectory(Path.Combine(bookLocation, "Messages", $"{currentFragmentId}"));
                Directory.CreateDirectory(Path.Combine(bookLocation, "Keys", $"{currentFragmentId}"));
            }

            return bookLocation;
        }
        public static string CreateSchemaFile(string bookLocation, string fileName, string schemaRawData)
        {
            string schemaDirLocation = Path.Combine(bookLocation, "Schemas");
            if (!Directory.Exists(schemaDirLocation))
            {
                Directory.CreateDirectory(schemaDirLocation);
            }

            var schemaLocation = Path.Combine(schemaDirLocation, fileName);
            File.WriteAllText(schemaLocation, schemaRawData);

            return schemaLocation;

        }
        public static void RenameStorageDirectory(string location, string suffix)
        {
            Directory.Move(location, $"{location}_{suffix}");
        }

        private static void UpdateSchemaInTenantsList(ConcurrentDictionary<string, Tenant> tenants, string rootPath)
        {
            string tenantRootPath = Path.Combine(rootPath, "Storage", "Tenants");
            foreach (var tenant in tenants)
            {
                foreach (var product in tenant.Value.Products)
                {
                    foreach (var component in product.Value.Components)
                    {
                        foreach (var book in component.Value.Books)
                        {
                            string schemaFileLocation = Path.Combine(tenantRootPath, tenant.Key, product.Key, component.Key, book.Key, "Schemas", $"{book.Value.Schema.Name}-{book.Value.Schema.Version}.andyxschema");
                            if (File.Exists(schemaFileLocation))
                                book.Value.Schema.SchemaRawData = File.ReadAllText(schemaFileLocation);
                        }
                    }
                }
            }
        }
    }
}
