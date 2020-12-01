using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories
{
    public class BookRepository : TenantRepository, IRepository<Book>
    {
        private readonly string _tenantName;
        private readonly string _productName;
        private readonly string _componentName;
        private ConcurrentDictionary<string, Book> _books;

        public BookRepository(string tenantName, string productName, string componentName)
        {
            _tenantName = tenantName;
            _productName = productName;
            _componentName = componentName;

            _books = base.GetBooks(tenantName, productName, componentName);
        }
        public void Add(string key, Book entity)
        {
            if (_books.TryAdd(key, entity))
                base.SaveChanges();
        }

        public bool Delete(string key)
        {
            if (_books.TryRemove(key, out _))
            {
                base.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Edit(string key, Book entity)
        {
            if (_books.ContainsKey(key))
            {
                _books[key] = entity;
                base.SaveChanges();
                return true;
            }
            return false;
        }

        public Book Get(string key)
        {
            if (_books.ContainsKey(key))
                return _books[key];
            throw new Exception($"There is no book registered with name andyx://{_tenantName}/{_productName}/{_componentName}/{key}");
        }

        ConcurrentDictionary<string, Book> IRepository<Book>.GetAll()
        {
            return _books;
        }
    }
}
