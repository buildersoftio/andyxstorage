using Buildersoft.Andy.X.Storage.Data.Model.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Readers
{
    public class Reader
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ReaderTypes ReaderTypes { get; set; }
        public ReaderAs ReaderAs { get; set; }
        public ConcurrentDictionary<string, string> MessagesAcked { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Reader()
        {
            Id = Guid.NewGuid();
            MessagesAcked = new ConcurrentDictionary<string, string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
