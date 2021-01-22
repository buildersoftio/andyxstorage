using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.IO.Storage.Tenants;
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
            Book book;
            string fragmentId, bookLocation;
            GetBookInfos(messageStoredArgs.Tenant, messageStoredArgs.Product, messageStoredArgs.Component, messageStoredArgs.Book, out book, out fragmentId, out bookLocation);

            if (MessageConfigFile.SaveMessageFile(fragmentId, bookLocation, messageStoredArgs.MessageId, messageStoredArgs.Message) != true)
                _logger.LogError($"{messageStoredArgs.Tenant}/{messageStoredArgs.Product}/{messageStoredArgs.Component}/{messageStoredArgs.Book}/messages/{messageStoredArgs.MessageId}: failed");

            _logger.LogInformation($"{messageStoredArgs.Tenant}/{messageStoredArgs.Product}/{messageStoredArgs.Component}/{messageStoredArgs.Book}/messages/{messageStoredArgs.MessageId}: stored");

            // This line will be executed in a different thread (This logic should be centralized inside a background service e.g. FragmentationService);
            CheckFragmentation(bookLocation, book);
        }

        public void StoreMessageLogToReader(MessageLogedArgs messageAcknowledgedArgs)
        {
            Book book;
            string fragmentId, bookLocation;
            GetBookInfos(messageAcknowledgedArgs.Tenant, messageAcknowledgedArgs.Product, messageAcknowledgedArgs.Component, messageAcknowledgedArgs.Book, out book, out fragmentId, out bookLocation);

            string logLine = $"{messageAcknowledgedArgs.Date.ToString("yyyy-MMM-dd HH:mm:ss")}\t{messageAcknowledgedArgs.MessageId}\t{messageAcknowledgedArgs.Log}";
            if (ReaderConfigFile.StoreLogInReader(bookLocation, messageAcknowledgedArgs.Reader, logLine) != true)
                _logger.LogError($"{messageAcknowledgedArgs.Tenant}/{messageAcknowledgedArgs.Product}/{messageAcknowledgedArgs.Component}/{messageAcknowledgedArgs.Book}/messages/{messageAcknowledgedArgs.MessageId}: failed to log in reader {messageAcknowledgedArgs.Reader}");
        }

        private void GetBookInfos(string tenant, string product, string component, string book, out Book bookDetail, out string fragmentId, out string bookLocation)
        {
            _bookRepository = new BookRepository(tenant, product, component);
            bookDetail = _bookRepository.Get(book);
            fragmentId = bookDetail.Fragmentation.CurrentFragmentId;
            bookLocation = TenantConfigFile.CreateBookLocation(tenant, product, component, book, fragmentId);
        }

        private void CheckFragmentation(string bookLocation, Book book)
        {
            string messageDirLocation = Path.Combine(bookLocation, "Messages", $"{book.Fragmentation.CurrentFragmentId}");
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
