using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.Events.Agents
{
    public class AgentDisconnectedArgs
    {
        public string Agnet { get; set; }
        public Guid AgentId { get; set; }

        public ConcurrentDictionary<string, Tenant> Tenants { get; set; }
    }
}
