using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Service.Connection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Core.Repository.Connection
{
    public class XNodeConnectionRepository : IXNodeConnectionRepository
    {
        private readonly ILogger<XNodeConnectionRepository> logger;
        ConcurrentDictionary<string, XNodeEventService> agents;
        public XNodeConnectionRepository(ILogger<XNodeConnectionRepository> logger)
        {
            this.logger = logger;
            agents = new ConcurrentDictionary<string, XNodeEventService>();
        }
        public void AddService(string key, XNodeEventService service)
        {
            if (agents.TryAdd(key, service))
                logger.LogInformation($"ANDYX-STORAGE#AGENT|{key}|STORED");
        }

        public XNodeEventService GetService(string key)
        {
            if (agents.ContainsKey(key))
                return agents[key];

            return null;
        }
    }
}
