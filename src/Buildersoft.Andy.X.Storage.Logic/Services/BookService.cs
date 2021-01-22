using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books.Schemas;
using Buildersoft.Andy.X.Storage.IO.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class BookService : IBookService
    {
        private readonly ILogger<BookService> _logger;
        private BookRepository _bookRepository;
        public BookService(ILogger<BookService> logger)
        {
            _logger = logger;
        }

        public void CreateBook(BookCreatedArgs bookCreatedArgs)
        {
            _bookRepository = new BookRepository(bookCreatedArgs.TenantName, bookCreatedArgs.ProductName, bookCreatedArgs.ComponentName);

            Book book;
            if (_bookRepository.GetBooks(bookCreatedArgs.TenantName, bookCreatedArgs.ProductName, bookCreatedArgs.ComponentName).ContainsKey(bookCreatedArgs.BookName) == true)
                throw new Exception($"There is a book registered with this name andyx://{bookCreatedArgs.TenantName}/{bookCreatedArgs.ProductName}/{bookCreatedArgs.ComponentName}/{bookCreatedArgs.BookName}");

            string bookLocation = TenantConfigFile.CreateBookLocation(bookCreatedArgs.TenantName, bookCreatedArgs.ProductName, bookCreatedArgs.ComponentName, bookCreatedArgs.BookName);

            book = new Book()
            {
                Id = bookCreatedArgs.BookId,
                Name = bookCreatedArgs.BookName,
                DataType = bookCreatedArgs.DataType,
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Schema = new Schema()
                {
                    Name = bookCreatedArgs.Schema.Name,
                    Version = bookCreatedArgs.Schema.Version,
                    SchemaValidationStatus = bookCreatedArgs.Schema.SchemaValidationStatus,
                    CreatedDate = bookCreatedArgs.Schema.CreatedDate,
                    ModifiedDate = bookCreatedArgs.Schema.ModifiedDate,
                    SchemaRawData = "STORED"
                },
                Location = bookLocation
            };

            TenantConfigFile.CreateSchemaFile(bookLocation, $"{bookCreatedArgs.Schema.Name}-{bookCreatedArgs.Schema.Version}.andyxschema", bookCreatedArgs.Schema.SchemaRawData);
            _bookRepository.Add(bookCreatedArgs.BookName, book);

            _logger.LogInformation($"{bookCreatedArgs.TenantName}/{bookCreatedArgs.ProductName}/{bookCreatedArgs.ComponentName}/{bookCreatedArgs.BookName}: stored");
        }

        public bool DeleteBook(BookDeletedArgs bookDeletedArgs)
        {
            throw new NotImplementedException();
        }

        public Book ReadBook(BookReadArgs bookReadArgs)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, Book> ReadBooks()
        {
            throw new NotImplementedException();
        }

        public Book UpdateBook(BookUpdatedArgs bookDeletedArgs)
        {
            throw new NotImplementedException();
        }

        public void UpdateBookSchema(BookSchemaUpdatedArgs bookSchemaUpdatedArgs)
        {
            _bookRepository = new BookRepository(bookSchemaUpdatedArgs.TenantName, bookSchemaUpdatedArgs.ProductName, bookSchemaUpdatedArgs.ComponentName);

            if (_bookRepository.GetBooks(bookSchemaUpdatedArgs.TenantName, bookSchemaUpdatedArgs.ProductName, bookSchemaUpdatedArgs.ComponentName).ContainsKey(bookSchemaUpdatedArgs.BookName) == true)
            {
                Book book = _bookRepository.Get(bookSchemaUpdatedArgs.BookName);
                book.Schema = bookSchemaUpdatedArgs.Schema;

                string bookLocation = TenantConfigFile.CreateBookLocation(bookSchemaUpdatedArgs.TenantName, bookSchemaUpdatedArgs.ProductName, bookSchemaUpdatedArgs.ComponentName, bookSchemaUpdatedArgs.BookName);
                TenantConfigFile.CreateSchemaFile(bookLocation, $"{bookSchemaUpdatedArgs.Schema.Name}-{bookSchemaUpdatedArgs.Schema.Version}.andyxschema", bookSchemaUpdatedArgs.Schema.SchemaRawData);

                _bookRepository.Edit(bookSchemaUpdatedArgs.BookName, book);

                _logger.LogInformation($"{bookSchemaUpdatedArgs.TenantName}/{bookSchemaUpdatedArgs.ProductName}/{bookSchemaUpdatedArgs.ComponentName}/{bookSchemaUpdatedArgs.BookName}/schema/{bookSchemaUpdatedArgs.Schema.Name}-{bookSchemaUpdatedArgs.Schema.Version}: updated");
            }

        }
    }
}
