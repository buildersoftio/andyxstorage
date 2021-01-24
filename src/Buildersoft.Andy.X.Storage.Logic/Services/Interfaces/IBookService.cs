using Buildersoft.Andy.X.Storage.Data.Model.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books.Schemas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services.Interfaces
{
    public interface IBookService
    {
        void CreateBook(BookCreatedArgs bookCreatedArgs);
        Book UpdateBook(BookUpdatedArgs bookDeletedArgs);
        Book ReadBook(BookReadArgs bookReadArgs);
        ConcurrentDictionary<string, Book> ReadBooks();
        bool DeleteBook(BookDeletedArgs bookDeletedArgs);

        void UpdateBookSchema(BookSchemaUpdatedArgs bookSchemaUpdatedArgs);

    }
}
