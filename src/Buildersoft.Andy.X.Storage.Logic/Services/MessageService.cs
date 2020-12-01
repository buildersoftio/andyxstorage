using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;
        private BookRepository _bookRepository;

        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }

        public void StoreMessage(MessageStoredArgs messageStoredArgs)
        {
            _bookRepository = new BookRepository(messageStoredArgs.Tenant, messageStoredArgs.Product, messageStoredArgs.Component);
            var book = _bookRepository.Get(messageStoredArgs.Book);

            var fragmentId = book.Fragmentation.CurrentFragmentId;
            var bookLocation = TenantConfigFile.CreateBookLocation(messageStoredArgs.Tenant, messageStoredArgs.Product, messageStoredArgs.Component, messageStoredArgs.Book, fragmentId);

            if (MessageConfigFile.SaveMessageFile(fragmentId, bookLocation, messageStoredArgs.MessageId, messageStoredArgs.Message) != true)
                _logger.LogError($"{messageStoredArgs.Tenant}/{messageStoredArgs.Product}/{messageStoredArgs.Component}/{messageStoredArgs.Book}/messages/{messageStoredArgs.MessageId}: failed");

            _logger.LogInformation($"{messageStoredArgs.Tenant}/{messageStoredArgs.Product}/{messageStoredArgs.Component}/{messageStoredArgs.Book}/messages/{messageStoredArgs.MessageId}: stored");

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

                _bookRepository.SaveChanges();
                _logger.LogInformation($"{book.Name} is fragmented,current fragmentation id: {book.Fragmentation.CurrentFragmentId}");
            }
        }
    }
}
