using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
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
            if (_bookRepository.GetAll().ContainsKey(bookCreatedArgs.BookName) == true)
                throw new Exception($"There is a book registered with this name andyx://{bookCreatedArgs.TenantName}/{bookCreatedArgs.ProductName}/{bookCreatedArgs.ComponentName}/{bookCreatedArgs.BookName}");

            string bookLocation = TenantConfigFile.CreateBookLocation(bookCreatedArgs.TenantName, bookCreatedArgs.ProductName, bookCreatedArgs.ComponentName, bookCreatedArgs.BookName);

            book = new Book()
            {
                Id = bookCreatedArgs.BookId,
                Name = bookCreatedArgs.BookName,
                DataType = bookCreatedArgs.DataType,
                ModifiedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                Location = bookLocation
            };

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
    }
}
