using System;

namespace Buildersoft.Andy.X.Storage.Model.App.Messages
{
    public class UnprocessedMessage
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }
        public Guid MessageId { get; set; }
        public string PartitionFile { get; set; }
    }
}
