using Buildersoft.Andy.X.Storage.Core.Service.Connection;

namespace Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection
{
    public interface IXNodeConnectionRepository
    {
        public XNodeEventService GetService(string key);
        public void AddService(string key, XNodeEventService service);
    }
}
