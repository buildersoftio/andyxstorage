using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        void Add(string key, T entity);
        bool Edit(string key, T entity);
        T Get(string key);
        ConcurrentDictionary<string, T> GetAll();
        bool Delete(string key);
    }
}
