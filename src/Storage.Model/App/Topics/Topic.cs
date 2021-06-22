using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.App.Producers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.Model.App.Topics
{
    public class Topic
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ConcurrentDictionary<string, Message> Messages { get; set; }
        public ConcurrentDictionary<string, Producer> Producers { get; set; }
        public ConcurrentDictionary<string, Consumer> Consumers { get; set; }

        public int PartitionIndex { get; set; }
        public string ActiveMessagePartitionFile { get; set; }
        public string PartitionDate { get; set; }
        public List<string> MessagePartitionFiles { get; set; }

        public Schema Schema { get; set; }

        public Topic()
        {
            Messages = new ConcurrentDictionary<string, Message>();
            Producers = new ConcurrentDictionary<string, Producer>();
            Consumers = new ConcurrentDictionary<string, Consumer>();

            Schema = new Schema();

            PartitionIndex = 1;
            ActiveMessagePartitionFile = $"msg_{DateTime.Now:MM_yyyy}_partition_{PartitionIndex}.xand";
            PartitionDate = DateTime.Now.ToString("dd-MM-yyyy");

            MessagePartitionFiles = new List<string>();

        }
    }

    public class Schema
    {
        public string Name { get; set; }
        public bool IsSchemaValidated { get; set; }
        public string SchemaRaw { get; set; }
        public int Version { get; set; }

        public Schema()
        {
            Version = 0;
            IsSchemaValidated = false;
        }
    }
}
