using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Service.XNodes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Core.Repository.Connection
{
    public class XNodeConnectionRepository : IXNodeConnectionRepository
    {
        private readonly ILogger<XNodeConnectionRepository> logger;
        ConcurrentDictionary<string, ConcurrentDictionary<string, XNodeEventService>> agents;
        public XNodeConnectionRepository(ILogger<XNodeConnectionRepository> logger)
        {
            this.logger = logger;
            agents = new ConcurrentDictionary<string, ConcurrentDictionary<string, XNodeEventService>>();
        }
        public void AddService(string xNode, string key, XNodeEventService service)
        {
            if (agents.ContainsKey(xNode) != true)
                agents.TryAdd(xNode, new ConcurrentDictionary<string, XNodeEventService>());

            if (agents[xNode].TryAdd(key, service))
                logger.LogInformation($"ANDYX-STORAGE#AGENT|{key}|STORED");
        }

        public ConcurrentDictionary<string, ConcurrentDictionary<string, XNodeEventService>> GetAllServices()
        {
            return agents;
        }

        public XNodeEventService GetService(string xNode, string key)
        {
            if (agents.ContainsKey(xNode) != true)
                return null;

            if (agents[xNode].ContainsKey(key))
                return agents[xNode][key];

            return null;
        }
    }
}
