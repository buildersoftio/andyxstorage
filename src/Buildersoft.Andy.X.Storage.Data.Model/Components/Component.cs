using Buildersoft.Andy.X.Storage.Data.Model.Books;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Components
{
    public class Component
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public ConcurrentDictionary<string, Book> Books { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Location { get; set; }

        public Component()
        {
            Books = new ConcurrentDictionary<string, Book>();
            IsActive = true;
        }
        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
