using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Components;
using Buildersoft.Andy.X.Storage.Data.Model.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces
{
    public interface ITenantRepository
    {
        ConcurrentDictionary<string, Tenant> GetAll();
        void AddTenant(Tenant tenant);
        Tenant UpdateTenant(string tenantName, Tenant tenant);
        bool DeleteTenant(string tenantName);
        ConcurrentDictionary<string, Product> GetProducts(string tenantName);
        ConcurrentDictionary<string, Component> GetComponents(string tenantName, string productName);
        ConcurrentDictionary<string, Book> GetBooks(string tenantName, string productName, string componentName);
        Book GetBook(string tenantName, string productName, string componentName, string bookName);
        bool Update(ConcurrentDictionary<string, Tenant> data);
        bool SaveChanges();
    }
}
