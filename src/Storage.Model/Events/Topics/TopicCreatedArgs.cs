using Buildersoft.Andy.X.Storage.Model.App.Topics;
using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Topics
{
    public class TopicCreatedArgs
    {
        public Guid TopicId { get; set; }
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }
        public string TopicName { get; set; }

        public Schema Schema { get; set; }
    }
}
