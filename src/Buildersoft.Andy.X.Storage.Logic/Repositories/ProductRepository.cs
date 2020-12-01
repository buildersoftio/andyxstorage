using Buildersoft.Andy.X.Storage.Data.Model.Products;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories
{
    public class ProductRepository : TenantRepository, IRepository<Product>
    {
        private readonly string _tenantName;
        private ConcurrentDictionary<string, Product> _products;
        public ProductRepository(string tenantName)
        {
            _tenantName = tenantName;
            _products = base.GetProducts(tenantName);
        }

        public void Add(string key, Product entity)
        {
            if (_products.TryAdd(key, entity))
                base.SaveChanges();
        }

        public bool Delete(string key)
        {
            if (_products.TryRemove(key, out _))
            {
                base.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Edit(string key, Product entity)
        {
            if (_products.ContainsKey(key))
            {
                _products[key] = entity;
                base.SaveChanges();
                return true;
            }
            return false;
        }

        public Product Get(string key)
        {
            if (_products.ContainsKey(key))
                return _products[key];
            throw new Exception($"There is no product registered with name andyx://{_tenantName}/{key}");
        }

        public new ConcurrentDictionary<string, Product> GetAll()
        {
            return _products;
        }
    }
}
