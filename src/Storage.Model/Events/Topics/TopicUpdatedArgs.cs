using Buildersoft.Andy.X.Storage.Model.App.Topics;
using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Topics
{
    public class TopicUpdatedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Schema Schema { get; set; }
    }
}
