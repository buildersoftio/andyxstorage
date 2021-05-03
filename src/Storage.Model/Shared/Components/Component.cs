using Buildersoft.Andy.X.Storage.Model.Shared.Topics;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.Shared.Components
{
    public class Component
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; }
        public ConcurrentDictionary<string, Topic> Topics { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Location { get; set; }

        public Component()
        {
            Topics = new ConcurrentDictionary<string, Topic>();
            Status = true;
        }
    }
}
