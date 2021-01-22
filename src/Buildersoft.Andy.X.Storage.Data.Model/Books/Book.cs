using Buildersoft.Andy.X.Storage.Data.Model.Messages;
using Buildersoft.Andy.X.Storage.Data.Model.Readers;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Data.Model.Books
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ConcurrentDictionary<string, Message> Messages { get; set; }
        public ConcurrentDictionary<string, Reader> Readers { get; set; }
        
        public Schema Schema { get; set; }

        public Fragmentation Fragmentation { get; set; }
        public DataTypes DataType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Location { get; set; }

        public Book()
        {
            Id = Guid.NewGuid();
            Messages = new ConcurrentDictionary<string, Message>();
            Readers = new ConcurrentDictionary<string, Reader>();
            Schema = new Schema();

            Fragmentation = new Fragmentation();
        }

        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
