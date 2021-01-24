using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class BookEventHandler
    {
        private readonly NodeDataStorageService _service;
        private readonly IBookService _bookService;
        public BookEventHandler(NodeDataStorageService service, IBookService bookService)
        {
            _service = service;
            _bookService = bookService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.BookCreated += DataStorageService_BookCreated;
            _service.BookRead += DataStorageService_BookRead;
            _service.BookUpdated += DataStorageService_BookUpdated;
            _service.BookDeleted += DataStorageService_BookDeleted;

            _service.BookSchemaUpdated += _service_BookSchemaUpdated;
        }

        private void DataStorageService_BookDeleted(Data.Model.Events.Books.BookDeletedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_BookUpdated(Data.Model.Events.Books.BookUpdatedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_BookRead(Data.Model.Events.Books.BookReadArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_BookCreated(Data.Model.Events.Books.BookCreatedArgs obj)
        {
            _bookService.CreateBook(obj);
        }
        private void _service_BookSchemaUpdated(Data.Model.Events.Books.Schemas.BookSchemaUpdatedArgs obj)
        {
            _bookService.UpdateBookSchema(obj);
        }
    }
}
