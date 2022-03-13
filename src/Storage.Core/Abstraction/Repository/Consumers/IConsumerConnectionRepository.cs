using Buildersoft.Andy.X.Storage.Model.App.Consumers;

namespace Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Consumers
{
    public interface IConsumerConnectionRepository
    {
        Consumer GetConsumerById(string id);
        void AddConsumer(string id, Consumer consumer);
        void AddConsumerConnection(string id);
        void RemoveConsumer(string id);
        void RemoveConsumerConnection(string id);
    }
}
