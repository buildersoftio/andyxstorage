using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Messages;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Repositories
{
    public class MessageRepository : TenantRepository, IRepository<Message>
    {
        private readonly string _tenantName;
        private readonly string _productName;
        private readonly string _componentName;
        private readonly string _bookName;
        private ConcurrentDictionary<string, Message> _messages;

        public MessageRepository(string tenantName, string productName, string componentName, string bookName)
        {
            _tenantName = tenantName;
            _productName = productName;
            _componentName = componentName;
            _bookName = bookName;

            //TODO... For now we are not focus on querying books.
        }

        public void Add(string key, Message entity)
        {
            throw new NotImplementedException();
        }

        public bool Delete(string key)
        {
            throw new NotImplementedException();
        }

        public bool Edit(string key, Message entity)
        {
            // No update to the message is alowed
            return false;
        }

        public Message Get(string key)
        {
            throw new NotImplementedException();
        }

        ConcurrentDictionary<string, Message> IRepository<Message>.GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
