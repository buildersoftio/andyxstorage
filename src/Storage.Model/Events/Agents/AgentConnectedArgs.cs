using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Agents
{
    public class AgentConnectedArgs
    {
        public string Agent { get; set; }
        public Guid AgentId { get; set; }
    }
}
