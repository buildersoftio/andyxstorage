using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Consumers;
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
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes
{
    public class XNodeEventService
    {
        private readonly ILogger<SystemService> logger;
        private readonly IXNodeConnectionRepository xNodeConnectionRepository;
        private readonly TenantIOService tenantIOService;
        private readonly ProducerIOService producerIOService;
        private readonly ConsumerIOService consumerIOService;
        private readonly MessageIOService messageIOService;
        private readonly IConsumerConnectionRepository consumerConnectionRepository;

        private HubConnection _connection;

        public event Action<AgentConnectedArgs> StorageConnected;
        public event Action<AgentDisconnectedArgs> StorageDisconnected;

        public event Action<TenantCreatedArgs> TenantCreated;
        public event Action<TenantUpdatedArgs> TenantUpdated;

        public event Action<TenantTokenCreatedArgs> TenantTokenCreated;
        public event Action<TenantTokenRevokedArgs> TenantTokenRevoked;

        public event Action<ProductCreatedArgs> ProductCreated;
        public event Action<ProductUpdatedArgs> ProductUpdated;

        public event Action<ComponentCreatedArgs> ComponentCreated;
        public event Action<ComponentUpdatedArgs> ComponentUpdated;

        public event Action<ComponentTokenCreatedArgs> ComponentTokenCreated;
        public event Action<ComponentTokenRevokedArgs> ComponentTokenRevoked;

        public event Action<TopicCreatedArgs> TopicCreated;
        public event Action<TopicUpdatedArgs> TopicUpdated;

        public event Action<MessageStoredArgs> MessageStored;
        public event Action<List<MessageStoredArgs>> MessagesStored;
        public event Action<MessageAcknowledgedArgs> MessageAcknowledged;

        public event Action<ProducerConnectedArgs> ProducerConnected;
        public event Action<ProducerDisconnectedArgs> ProducerDisconnected;

        public event Action<ConsumerConnectedArgs> ConsumerConnected;
        public event Action<ConsumerDisconnectedArgs> ConsumerDisconnected;
        public event Action<ConsumerConnectedArgs> ConsumerUnacknowledgedMessagesRequested;


        private AgentEventHandler agentEventHandler;
        private TenantEventHandler tenantEventHandler;
        private ProducerEventHandler producerEventHandler;
        private ConsumerEventHandler consumerEventHandler;
        private MessageEventHandler messageEventHandler;

        private string agentId;
        private readonly XNodeConfiguration nodeConfig;
        private readonly PartitionConfiguration partitionConfiguration;

        public XNodeEventService(ILogger<SystemService> logger,
            string agentId,
            XNodeConfiguration nodeConfig,
            DataStorageConfiguration dataStorageConfig,
            PartitionConfiguration partitionConfiguration,
            AgentConfiguration agentConfiguration,
            IXNodeConnectionRepository xNodeConnectionRepository,
            TenantIOService tenantIOService,
            ProducerIOService producerIOService,
            ConsumerIOService consumerIOService,
            MessageIOService messageIOService,
            IConsumerConnectionRepository consumerConnectionRepository)
        {
            this.logger = logger;
            this.xNodeConnectionRepository = xNodeConnectionRepository;
            this.tenantIOService = tenantIOService;
            this.producerIOService = producerIOService;
            this.consumerIOService = consumerIOService;
            this.messageIOService = messageIOService;
            this.consumerConnectionRepository = consumerConnectionRepository;
            this.agentId = agentId;
            this.nodeConfig = nodeConfig;
            this.partitionConfiguration = partitionConfiguration;


            var provider = new XNodeConnectionProvider(nodeConfig, dataStorageConfig, agentConfiguration, agentId);
            _connection = provider.GetHubConnection();

            _connection.Closed += _connection_Closed;
            _connection.Reconnected += _connection_Reconnected;
            _connection.Reconnecting += _connection_Reconnecting;

            _connection.On<AgentConnectedArgs>("StorageConnected", connectedArgs => StorageConnected?.Invoke(connectedArgs));
            _connection.On<AgentDisconnectedArgs>("StorageDisconnected", disconnectedArgs => StorageDisconnected?.Invoke(disconnectedArgs));

            _connection.On<TenantCreatedArgs>("TenantCreated", tenantCreated => TenantCreated?.Invoke(tenantCreated));
            _connection.On<TenantUpdatedArgs>("TenantUpdated", tenantUpdated => TenantUpdated?.Invoke(tenantUpdated));

            _connection.On<TenantTokenCreatedArgs>("TenantTokenCreated", tenantTokenCreated => TenantTokenCreated?.Invoke(tenantTokenCreated));
            _connection.On<TenantTokenRevokedArgs>("TenantTokenRevoked", tenantTokenRevoked => TenantTokenRevoked?.Invoke(tenantTokenRevoked));

            _connection.On<ProductCreatedArgs>("ProductCreated", productCreated => ProductCreated?.Invoke(productCreated));
            _connection.On<ProductUpdatedArgs>("ProductUpdated", productUpdated => ProductUpdated?.Invoke(productUpdated));

            _connection.On<ComponentCreatedArgs>("ComponentCreated", componentCreated => ComponentCreated?.Invoke(componentCreated));
            _connection.On<ComponentUpdatedArgs>("ComponentUpdated", componentUpdated => ComponentUpdated?.Invoke(componentUpdated));

            _connection.On<ComponentTokenCreatedArgs>("ComponentTokenCreated", componentTokenCreated => ComponentTokenCreated?.Invoke(componentTokenCreated));
            _connection.On<ComponentTokenRevokedArgs>("ComponentTokenRevoked", componentTokenRevoked => ComponentTokenRevoked?.Invoke(componentTokenRevoked));

            _connection.On<TopicCreatedArgs>("TopicCreated", topicCreated => TopicCreated?.Invoke(topicCreated));
            _connection.On<TopicUpdatedArgs>("TopicUpdated", topicUpdated => TopicUpdated?.Invoke(topicUpdated));

            _connection.On<ProducerConnectedArgs>("ProducerConnected", producerConnected => ProducerConnected?.Invoke(producerConnected));
            _connection.On<ProducerDisconnectedArgs>("ProducerDisconnected", producerDisconnected => ProducerDisconnected?.Invoke(producerDisconnected));

            _connection.On<ConsumerConnectedArgs>("ConsumerConnected", consumerConnected => ConsumerConnected?.Invoke(consumerConnected));
            _connection.On<ConsumerDisconnectedArgs>("ConsumerDisconnected", consumerDisconnected => ConsumerDisconnected?.Invoke(consumerDisconnected));
            _connection.On<ConsumerConnectedArgs>("ConsumerUnacknowledgedMessagesRequested", consumerConnected => ConsumerUnacknowledgedMessagesRequested?.Invoke(consumerConnected));
            _connection.On<MessageAcknowledgedArgs>("MessageAcknowledged", messageAcked => MessageAcknowledged?.Invoke(messageAcked));

            _connection.On<MessageStoredArgs>("MessageStored", msgStored => MessageStored?.Invoke(msgStored));
            _connection.On<List<MessageStoredArgs>>("MessagesStored", msgStored => MessagesStored?.Invoke(msgStored));

            InitializeEventHandlers();
            ConnectAsync();

            xNodeConnectionRepository.AddService(nodeConfig.ServiceUrl, agentId, this);
        }

        private Task _connection_Closed(Exception arg)
        {
            logger.LogError($"Agent connection is closed, details {arg.Message}");

            // try to reconnect
            ConnectAsync();

            return Task.CompletedTask;
        }

        private Task _connection_Reconnected(string arg)
        {
            logger.LogInformation($"Agent with id {Guid.NewGuid()} is connected");
            return Task.CompletedTask;
        }

        private Task _connection_Reconnecting(Exception arg)
        {
            logger.LogWarning($"Agent connection is lost, agent is reconnecting to node, details {arg.Message}");
            return Task.CompletedTask;
        }

        private void InitializeEventHandlers()
        {
            agentEventHandler = new AgentEventHandler(logger, this, tenantIOService);
            tenantEventHandler = new TenantEventHandler(logger, this, tenantIOService);
            producerEventHandler = new ProducerEventHandler(logger, this, producerIOService);
            consumerEventHandler = new ConsumerEventHandler(logger, this, consumerIOService, messageIOService, partitionConfiguration, consumerConnectionRepository);
            messageEventHandler = new MessageEventHandler(logger, this, messageIOService);
        }

        public IXNodeConnectionRepository GetXNodeConnectionRepository()
        {
            return xNodeConnectionRepository;
        }

        public string GetCurrentXNodeServiceUrl()
        {
            return nodeConfig.ServiceUrl;
        }
        public HubConnection GetHubConnection()
        {
            return _connection;
        }

        public async void ConnectAsync()
        {
            await _connection.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    // Details is not show for now...
                    //logger.LogError($"Error occurred during connection. Details: {task.Exception.Message}, {string.Join(",", task.Exception.InnerExceptions)}");

                    logger.LogError($"Error occurred during connection. Details: {task.Exception.Message}");

                    // retry connection
                    Thread.Sleep(3000);
                    logger.LogWarning($"Agent is reconnecting...");
                    ConnectAsync();
                }
            });
        }
    }
}
