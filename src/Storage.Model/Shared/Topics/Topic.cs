using Buildersoft.Andy.X.Storage.Model.Shared.Consumers;
using Buildersoft.Andy.X.Storage.Model.Shared.Messages;
using Buildersoft.Andy.X.Storage.Model.Shared.Producers;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.Shared.Topics
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ConcurrentDictionary<string, Message> Messages { get; set; }
        public ConcurrentDictionary<string, Producer> Producers { get; set; }
        public ConcurrentDictionary<string, Consumer> Consumers { get; set; }

        public Schema Schema { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Topic()
        {
            Messages = new ConcurrentDictionary<string, Message>();
            Producers = new ConcurrentDictionary<string, Producer>();
            Consumers = new ConcurrentDictionary<string, Consumer>();

            Schema = new Schema();
        }
    }

    public class Schema
    {
        public string Name { get; set; }

        public bool SchemaValidationStatus { get; set; }

        public string SchemaRawData { get; set; }
        public int Version { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Schema()
        {
            SchemaValidationStatus = false;
            Version = 1;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}
