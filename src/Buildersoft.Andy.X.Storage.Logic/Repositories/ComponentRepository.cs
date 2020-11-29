using Buildersoft.Andy.X.Storage.Data.Model.Components;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories
{
    public class ComponentRepository : TenantRepository, IRepository<Component>
    {
        private readonly string _tenantName;
        private readonly string _productName;
        private ConcurrentDictionary<string, Component> _components;

        public ComponentRepository(string tenantName, string productName)
        {
            _tenantName = tenantName;
            _productName = productName;
            _components = base.GetComponents(tenantName, productName);
        }

        public void Add(string key, Component entity)
        {
            if (_components.TryAdd(key, entity))
                base.SaveChanges();
        }

        public bool Delete(string key)
        {
            if (_components.TryRemove(key, out _))
            {
                base.SaveChanges();
                return true;
            }
            return false;
        }

        public Component Get(string key)
        {
            if (_components.ContainsKey(key))
                return _components[key];
            throw new Exception($"There is no component registered with name andyx://{_tenantName}/{_productName}/{key}");

        }

        ConcurrentDictionary<string, Component> IRepository<Component>.GetAll()
        {
            return _components;
        }
    }
}
