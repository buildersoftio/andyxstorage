using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Provider;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Events.Components;
using Buildersoft.Andy.X.Storage.Model.Events.Connections;
using Buildersoft.Andy.X.Storage.Model.Events.Consumers;
using Buildersoft.Andy.X.Storage.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.Model.Events.Producers;
using Buildersoft.Andy.X.Storage.Model.Events.Products;
using Buildersoft.Andy.X.Storage.Model.Events.Tenants;
using Buildersoft.Andy.X.Storage.Model.Events.Topics;
using Buildersoft.Andy.X.Storage.Model.Events.Topics.Schemas;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;

namespace Buildersoft.Andy.X.Storage.Core.Service.Connection
{
    public class XNodeEventService
    {
        private readonly ILogger<XNodeEventService> logger;
        private readonly IXNodeConnectionRepository xNodeConnectionRepository;

        private HubConnection _connection;

        public event Action<DataStorageConnectedArgs> DataStorageConnected;
        public event Action<DataStorageDisconnectedArgs> DataStorageDisconnected;

        public event Action<TenantCreatedArgs> TenantCreated;
        public event Action<TenantUpdatedArgs> TenantUpdated;

        public event Action<ProductCreatedArgs> ProductCreated;
        public event Action<ProductUpdatedArgs> ProductUpdated;

        public event Action<ComponentCreatedArgs> ComponentCreated;
        public event Action<ComponentUpdatedArgs> ComponentUpdated;

        public event Action<TopicCreatedArgs> TopicCreated;
        public event Action<SchemaUpdatedArgs> SchemaUpdated;

        public event Action<MessageStoredArgs> MessageStored;
        public event Action<MessageAcknowledgedArgs> MessageAcknowledged;

        public event Action<ProducerConnectedArgs> ProducerConnected;
        public event Action<ProducerDisconnectedArgs> ProducerDisconnected;

        public event Action<ConsumerConnectedArgs> ConsumerConnected;
        public event Action<ConsumerDisconnectedArgs> ConsumerDisconnected;

        public XNodeEventService(ILogger<XNodeEventService> logger,
            string agentId,
            XNodeConfiguration nodeConfig,
            DataStorageConfiguration dataStorageConfig,
            IXNodeConnectionRepository xNodeConnectionRepository)
        {
            this.logger = logger;
            this.xNodeConnectionRepository = xNodeConnectionRepository;

            var provider = new XNodeConnectionProvider(nodeConfig, dataStorageConfig, agentId);
            _connection = provider.GetHubConnection();

            _connection.On<DataStorageConnectedArgs>("DataStorageConnected", connectedArgs => DataStorageConnected?.Invoke(connectedArgs));
            _connection.On<DataStorageDisconnectedArgs>("DataStorageDisconnected", disconnectedArgs => DataStorageDisconnected?.Invoke(disconnectedArgs));

            _connection.On<TenantCreatedArgs>("TenantCreated", tenantCreated => TenantCreated?.Invoke(tenantCreated));
            _connection.On<TenantUpdatedArgs>("TenantUpdated", tenantUpdated => TenantUpdated?.Invoke(tenantUpdated));

            _connection.On<ProductCreatedArgs>("ProductCreated", productCreated => ProductCreated?.Invoke(productCreated));
            _connection.On<ProductUpdatedArgs>("ProductUpdated", productUpdated => ProductUpdated?.Invoke(productUpdated));

            _connection.On<ComponentCreatedArgs>("ComponentCreated", componentCreated => ComponentCreated?.Invoke(componentCreated));
            _connection.On<ComponentUpdatedArgs>("ComponentUpdated", componentUpdated => ComponentUpdated?.Invoke(componentUpdated));

            _connection.On<TopicCreatedArgs>("TopicCreated", topicCreated => TopicCreated?.Invoke(topicCreated));
            _connection.On<SchemaUpdatedArgs>("SchemaUpdated", schemaUpdated => SchemaUpdated?.Invoke(schemaUpdated));

            _connection.On<ProducerConnectedArgs>("ProducerConnected", producerConnected => ProducerConnected?.Invoke(producerConnected));
            _connection.On<ProducerDisconnectedArgs>("ProducerDisconnected", producerDisconnected => ProducerDisconnected?.Invoke(producerDisconnected));

            _connection.On<ConsumerConnectedArgs>("ConsumerConnected", consumerConnected => ConsumerConnected?.Invoke(consumerConnected));
            _connection.On<ConsumerDisconnectedArgs>("ConsumerDisconnected", consumerDisconnected => ConsumerDisconnected?.Invoke(consumerDisconnected));

            ConnectAsync();

            xNodeConnectionRepository.AddService(agentId, this);
        }

        public async void ConnectAsync()
        {
            await _connection.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    logger.LogError($"Error occurred during connection. Details: {task.Exception.Message}");
                }
            });
        }
    }
}
