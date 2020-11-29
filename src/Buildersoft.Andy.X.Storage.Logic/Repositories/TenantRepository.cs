using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Components;
using Buildersoft.Andy.X.Storage.Data.Model.Events;
using Buildersoft.Andy.X.Storage.Data.Model.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        ConcurrentDictionary<string, Tenant> _tenants;
        public TenantRepository()
        {
            //Get from configFile
            _tenants = TenantConfigFile.GetTenants();
        }

        public void AddTenant(Tenant tenant)
        {
            _tenants.TryAdd(tenant.Name, tenant);
            SaveChanges();
        }

        public bool DeleteTenant(string tenantName)
        {
            if (_tenants.TryRemove(tenantName, out _))
            {
                SaveChanges();
                return true;
            }
            return false;
        }

        public ConcurrentDictionary<string, Tenant> GetAll()
        {
            return _tenants;
        }

        public Book GetBook(string tenantName, string productName, string componentName, string bookName)
        {
            if (_tenants.ContainsKey(tenantName))
                if (_tenants[tenantName].Products.ContainsKey(productName))
                    if (_tenants[tenantName].Products[productName].Components.ContainsKey(componentName))
                        return _tenants[tenantName]
                            .Products[productName]
                            .Components[componentName]
                            .Books[bookName];

            throw new Exception($"There is no book registered with name andyx://{tenantName}/{productName}/{componentName}/{bookName}");
        }

        public ConcurrentDictionary<string, Book> GetBooks(string tenantName, string productName, string componentName)
        {
            if (_tenants.ContainsKey(tenantName))
                if (_tenants[tenantName].Products.ContainsKey(productName))
                    if (_tenants[tenantName].Products[productName].Components.ContainsKey(componentName))
                        return _tenants[tenantName]
                            .Products[productName]
                            .Components[componentName]
                            .Books;

            throw new Exception($"There is no component registered with name andyx://{tenantName}/{productName}/{componentName}");
        }

        public ConcurrentDictionary<string, Component> GetComponents(string tenantName, string productName)
        {
            if (_tenants.ContainsKey(tenantName))
                if (_tenants[tenantName].Products.ContainsKey(productName))
                    return _tenants[tenantName]
                        .Products[productName]
                        .Components;

            throw new Exception($"There is no product registered with name andyx://{tenantName}/{productName}");
        }

        public ConcurrentDictionary<string, Product> GetProducts(string tenantName)
        {
            if (_tenants.ContainsKey(tenantName))
                return _tenants[tenantName]
                    .Products;

            throw new Exception($"There is no tenant registered with name andyx:// {tenantName}");
        }

        public bool SaveChanges()
        {
            try
            {
                return TenantConfigFile.UpdateTenants(_tenants);

            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Update(ConcurrentDictionary<string, Tenant> data)
        {
            if (TenantConfigFile.UpdateTenants(data))
            {
                _tenants = TenantConfigFile.GetTenants();
                return true;
            }
            return false;
        }

        public Tenant UpdateTenant(string tenantName, Tenant tenant)
        {
            if (_tenants.TryUpdate(tenantName, tenant, new Tenant()))
            {
                return tenant;
            }
            throw new Exception($"Update failed at andyx:// {tenantName}");
        }
    }
}
