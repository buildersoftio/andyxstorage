using System;
using System.Collections.Concurrent;
using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;

namespace Buildersoft.Andy.X.Storage.Core.Repository.Consumers
{
    public class ConsumerConnectionRepository : IConsumerConnectionRepository
    {
        private readonly ConcurrentDictionary<string, Consumer> _consumersConnected;

        public ConsumerConnectionRepository()
        {
            _consumersConnected = new ConcurrentDictionary<string, Consumer>();
        }

        public Consumer GetConsumerById(string id)
        {
            return _consumersConnected.ContainsKey(id)
                ? _consumersConnected[id]
                : null;
        }

        public void AddConsumer(string id, Consumer consumer)
        {
            _consumersConnected.TryAdd(id, consumer);
        }

        public void AddConsumerConnection(string id)
        {
            if (_consumersConnected.ContainsKey(id))
                _consumersConnected[id].Connections.Add(Guid.NewGuid());
        }

        public void RemoveConsumer(string id)
        {
            if (!_consumersConnected.ContainsKey(id))
                return;

            if (_consumersConnected[id].Connections.Count == 0)
                _consumersConnected.TryRemove(id, out _);
        }

        public void RemoveConsumerConnection(string id)
        {
            if (!_consumersConnected.ContainsKey(id))
                return;

            if (_consumersConnected[id].Connections.Count != 0)
                _consumersConnected[id].Connections.RemoveAt(0);
        }
    }
}