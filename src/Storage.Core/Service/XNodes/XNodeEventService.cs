using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Provider;
using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Events.Agents;
using Buildersoft.Andy.X.Storage.Model.Events.Components;
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

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes
{
    public class XNodeEventService
    {
        private readonly ILogger<SystemService> logger;
        private readonly IXNodeConnectionRepository xNodeConnectionRepository;
        private readonly TenantIOService tenantIOService;
        private readonly ProducerIOService producerIOService;

        private HubConnection _connection;

        public event Action<AgentConnectedArgs> StorageConnected;
        public event Action<AgentDisconnectedArgs> StorageDisconnected;

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


        private AgentEventHandler agentEvnetHandler;
        private TenantEventHandler tenantEventHandler;
        private ProducerEventHandler producerEventHandler;

        private string agentId;

        public XNodeEventService(ILogger<SystemService> logger,
            string agentId,
            XNodeConfiguration nodeConfig,
            DataStorageConfiguration dataStorageConfig,
            AgentConfiguration agentConfiguration,
            IXNodeConnectionRepository xNodeConnectionRepository, 
            TenantIOService tenantIOService,
            ProducerIOService producerIOService)
        {
            this.logger = logger;
            this.xNodeConnectionRepository = xNodeConnectionRepository;
            this.tenantIOService = tenantIOService;
            this.producerIOService = producerIOService;
            this.agentId = agentId;

            var provider = new XNodeConnectionProvider(nodeConfig, dataStorageConfig, agentConfiguration, agentId);
            _connection = provider.GetHubConnection();

            _connection.On<AgentConnectedArgs>("StorageConnected", connectedArgs => StorageConnected?.Invoke(connectedArgs));
            _connection.On<AgentDisconnectedArgs>("StorageDisconnected", disconnectedArgs => StorageDisconnected?.Invoke(disconnectedArgs));

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

            InitializeEventHandlers();

            ConnectAsync();

            xNodeConnectionRepository.AddService(agentId, this);
        }

        private void InitializeEventHandlers()
        {
            agentEvnetHandler = new AgentEventHandler(logger, this, tenantIOService);
            tenantEventHandler = new TenantEventHandler(logger, this, tenantIOService);
            producerEventHandler = new ProducerEventHandler(logger, this, producerIOService);

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
