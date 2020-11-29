using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class MessageService : IMessageService
    {
        private readonly ITenantRepository _repository;
        private BookRepository _bookRepository;

        public MessageService(ITenantRepository tenantRepository)
        {
            _repository = tenantRepository;
        }

        public void StoreMessage(MessageStoredArgs messageStoredArgs)
        {
            var book = _repository.GetBook(messageStoredArgs.Tenant, messageStoredArgs.Product, messageStoredArgs.Component, messageStoredArgs.Book);
            var fragmentId = book.Fragmentation.CurrentFragmentId;
            var bookLocation = TenantConfigFile.CreateBookLocation(messageStoredArgs.Tenant, messageStoredArgs.Product, messageStoredArgs.Component, messageStoredArgs.Book, fragmentId);
            
            MessageConfigFile.SaveMessageFile(fragmentId, bookLocation, messageStoredArgs.MessageId, messageStoredArgs.Message);

            // This line will be executed in a different thread (This logic should be centralized inside a background service e.g. FragmentationService);
            CheckFragmentation(bookLocation, book);
        }

        private void CheckFragmentation(string bookLocation, Book book)
        {
            string messageDirLocation = Path.Combine(bookLocation, $"Messages_{book.Fragmentation.CurrentFragmentId}");
            int messagesInFragmentedMessageDir = Directory.GetFiles(messageDirLocation).Length;
            if (messagesInFragmentedMessageDir >= book.Fragmentation.MaxNumberOfRecordsForFragment)
            {
                book.Fragmentation.CurrentFragmentId = Randoms.Generate6Digits();
                _repository.SaveChanges();
            }
        }
    }
}
