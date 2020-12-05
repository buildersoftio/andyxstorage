using Buildersoft.Andy.X.Agents;
using Buildersoft.Andy.X.Agents.Models;
using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Data.Model.Events;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Books;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Components;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Products;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Readers;
using Buildersoft.Andy.X.Storage.Data.Model.Events.Tenants;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.Providers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.Services
{
    public class NodeDataStorageService
    {
        private readonly ILogger<NodeDataStorageService> _logger;
        private readonly HubConnection _connection;

        private BackgroundAgent _connectionAgent;

        public event Action<DataStorageConnectedArgs> DataStorageConnected;
        public event Action<DataStorageDisconnectedArgs> DataStorageDisconnected;

        public event Action<TenantCreatedArgs> TenantCreated;
        public event Action<TenantReadArgs> TenantRead;
        public event Action<TenantUpdatedArgs> TenantUpdated;
        public event Action<TenantDeletedArgs> TenantDeleted;

        public event Action<ProductCreatedArgs> ProductCreated;
        public event Action<ProductReadArgs> ProductRead;
        public event Action<ProductUpdatedArgs> ProductUpdated;
        public event Action<ProductDeletedArgs> ProductDeleted;

        public event Action<ComponentCreatedArgs> ComponentCreated;
        public event Action<ComponentReadArgs> ComponentRead;
        public event Action<ComponentUpdatedArgs> ComponentUpdated;
        public event Action<ComponentDeletedArgs> ComponentDeleted;

        public event Action<BookCreatedArgs> BookCreated;
        public event Action<BookReadArgs> BookRead;
        public event Action<BookUpdatedArgs> BookUpdated;
        public event Action<BookDeletedArgs> BookDeleted;

        public event Action<MessageStoredArgs> MessageStored;
        public event Action<MessageLogedArgs> MessageLogStored;

        public event Action<ReaderStoredArgs> ReaderConnectStored;
        public event Action<ReaderStoredArgs> ReaderDisconnectStored;

        public NodeDataStorageService(ILogger<NodeDataStorageService> logger, HubConnectionProvider connectionProvider)
        {
            _logger = logger;
            _connection = connectionProvider.GetHubConnection();

            _connection.On<DataStorageConnectedArgs>("DataStorageConnected", connectedArgs => DataStorageConnected?.Invoke(connectedArgs));
            _connection.On<DataStorageDisconnectedArgs>("DataStorageDisconnected", disconnectArgs => DataStorageDisconnected?.Invoke(disconnectArgs));

            _connection.On<TenantCreatedArgs>("TenantCreated", tenant => TenantCreated?.Invoke(tenant));
            _connection.On<TenantReadArgs>("TenantRead", tenant => TenantRead?.Invoke(tenant));
            _connection.On<TenantUpdatedArgs>("TenantUpdated", tenant => TenantUpdated?.Invoke(tenant));
            _connection.On<TenantDeletedArgs>("TenantDeleted", tenant => TenantDeleted?.Invoke(tenant));

            _connection.On<ProductCreatedArgs>("ProductCreated", product => ProductCreated?.Invoke(product));
            _connection.On<ProductReadArgs>("ProductRead", product => ProductRead?.Invoke(product));
            _connection.On<ProductUpdatedArgs>("ProductUpdated", product => ProductUpdated?.Invoke(product));
            _connection.On<ProductDeletedArgs>("ProductDeleted", product => ProductDeleted?.Invoke(product));

            _connection.On<ComponentCreatedArgs>("ComponentCreated", component => ComponentCreated?.Invoke(component));
            _connection.On<ComponentReadArgs>("ComponentRead", component => ComponentRead?.Invoke(component));
            _connection.On<ComponentUpdatedArgs>("ComponentUpdated", component => ComponentUpdated?.Invoke(component));
            _connection.On<ComponentDeletedArgs>("ComponentDeleted", component => ComponentDeleted?.Invoke(component));

            _connection.On<BookCreatedArgs>("BookCreated", book => BookCreated?.Invoke(book));
            _connection.On<BookReadArgs>("BookRead", book => BookRead?.Invoke(book));
            _connection.On<BookUpdatedArgs>("BookUpdated", book => BookUpdated?.Invoke(book));
            _connection.On<BookDeletedArgs>("BookDeleted", book => BookDeleted?.Invoke(book));

            _connection.On<MessageStoredArgs>("MessageStored", message => MessageStored?.Invoke(message));
            _connection.On<MessageLogedArgs>("MessageLogStored", message => MessageLogStored?.Invoke(message));
            _connection.On<ReaderStoredArgs>("ReaderConnectStored", reader => ReaderConnectStored(reader));
            _connection.On<ReaderStoredArgs>("ReaderDisconnectStored", reader => ReaderDisconnectStored(reader));

            _ = ConnectAsync();

            _connectionAgent = new BackgroundAgent()
                .Interval(new TimeSpan(0, 0, 10))
                .Name("andyx-data-storage-timer")
                .Build();

            _connectionAgent.TaskAssigned += ConnectionAgent_TaskAssigned;
            _connectionAgent.Start();
        }

        private void ConnectionAgent_TaskAssigned(object sender, TaskAssignedEventArgs e)
        {
            switch (_connection.State)
            {
                case HubConnectionState.Disconnected:
                    _logger.LogWarning($"Andy X Data Storage is trying to connect to Andy X Node. Connection state {_connection.State.ToString()}");
                    _ = ConnectAsync();
                    break;
                case HubConnectionState.Connected:
                    break;
                default:
                    break;
            }
        }

        public async Task ConnectAsync()
        {
            await _connection.StartAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    // Add exception like   
                    // Unable to connect to DataStorageHub in remote.
                    _logger.LogError($"Error occurred during connection. Details: {task.Exception.Message}");
                }
            });
        }

        public async Task SendStorageCurrentState(ConcurrentDictionary<string, Tenant> tenants)
        {
            await _connection.SendAsync("SendStorageCurrentState", tenants);
        }

        public async Task MessageStoredInDataStorage(MessageStoredArgs message)
        {
            await _connection.SendAsync("MessageStoredInDataStorage", message);
        }
    }
}
