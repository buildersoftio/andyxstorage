using Buildersoft.Andy.X.Storage.Core.Service.XNodes;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection
{
    public interface IXNodeConnectionRepository
    {
        public XNodeEventService GetService(string xNode, string key);
        public void AddService(string xNode, string key, XNodeEventService service);
        public ConcurrentDictionary<string, ConcurrentDictionary<string, XNodeEventService>> GetAllServices();
    }
}
