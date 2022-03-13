using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Consumers;
using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Commands.Consumer;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Model.Events.Consumers;
using Buildersoft.Andy.X.Storage.Model.Files;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class ConsumerEventHandler
    {
        private readonly ILogger<SystemService> _logger;
        private readonly XNodeEventService _xNodeEventService;
        private readonly ConsumerIOService _consumerIOService;
        private readonly MessageIOService _messageIOService;
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly IConsumerConnectionRepository _consumerConnectionRepository;

        private readonly ConcurrentDictionary<string, Task> _unacknowledgedMessageProcesses;

        public ConsumerEventHandler(
            ILogger<SystemService> logger,
            XNodeEventService xNodeEventService,
            ConsumerIOService consumerIOService,
            MessageIOService messageIOService,
            PartitionConfiguration partitionConfiguration,
            IConsumerConnectionRepository consumerConnectionRepository)
        {
            _logger = logger;
            _xNodeEventService = xNodeEventService;
            _consumerIOService = consumerIOService;
            _messageIOService = messageIOService;
            _partitionConfiguration = partitionConfiguration;
            _consumerConnectionRepository = consumerConnectionRepository;

            _unacknowledgedMessageProcesses = new ConcurrentDictionary<string, Task>();

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _xNodeEventService.ConsumerConnected += XNodeEventService_ConsumerConnected;
            _xNodeEventService.ConsumerDisconnected += XNodeEventService_ConsumerDisconnected;
            _xNodeEventService.ConsumerUnacknowledgedMessagesRequested += XNodeEventService_ConsumerUnacknowledgedMessagesRequested;
            _xNodeEventService.MessageAcknowledged += XNodeEventService_MessageAcknowledged;
        }

        private async void XNodeEventService_ConsumerConnected(ConsumerConnectedArgs obj)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            var consumer = new Consumer()
            {
                Id = obj.Id,
                Name = obj.ConsumerName,
                Tenant = obj.Tenant,
                Product = obj.Product,
                Component = obj.Component,
                Topic = obj.Topic,
                SubscriptionType = obj.SubscriptionType,
                ConsumerSettings = new ConsumerSettings() { InitialPosition = obj.InitialPosition },
                CreatedDate = DateTime.Now
            };
            _consumerIOService.TryCreateConsumerDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic, consumer);

            _consumerConnectionRepository.AddConsumer(consumerKey, consumer);
            _consumerConnectionRepository.AddConsumerConnection(consumerKey);

            // notify other nodes in cluster that a consumer has been disconnected
            await NotifyNodesForConsumerConnection(new NotifyConsumerConnection()
            {
                ConnectionType = ConnectionType.Connected,

                Tenant = obj.Tenant,
                Product = obj.Product,
                Component = obj.Component,
                Topic = obj.Topic,
                Id = obj.Id,
                ConsumerName = obj.ConsumerName,
                SubscriptionType = obj.SubscriptionType,
                InitialPosition = InitialPosition.Latest,
            });

        }

        private async void XNodeEventService_ConsumerDisconnected(ConsumerDisconnectedArgs obj)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            var consumer = new Consumer()
            {
                Id = obj.Id,
                Name = obj.ConsumerName,
                Tenant = obj.Tenant,
                Product = obj.Product,
                Component = obj.Component,
                Topic = obj.Topic,
                SubscriptionType = obj.SubscriptionType,
                CreatedDate = DateTime.Now
            };
            _consumerIOService.WriteDisconnectedConsumerLog(obj.Tenant, obj.Product, obj.Component, obj.Topic, consumer);

            var consumerState = _consumerConnectionRepository.GetConsumerById(consumerKey);
            _consumerConnectionRepository.RemoveConsumerConnection(consumerKey);

            // We will not remove the consumer, only when the sending of msgs is done
            // _consumerConnectionRepository.RemoveConsumer(consumerKey);

            if (obj.SubscriptionType != SubscriptionType.Shared && consumerState.StorageStateProperty.IsNewConsumer == false)
            {
                ReleaseUnacknoledgedMessageTasks(consumerKey, true);
            }

            // notify other nodes in cluster that a consumer has been disconnected
            await NotifyNodesForConsumerConnection(new NotifyConsumerConnection()
            {
                ConnectionType = ConnectionType.Disconnected,

                Tenant = obj.Tenant,
                Product = obj.Product,
                Topic = obj.Topic,
                Component = obj.Component,
                Id = obj.Id,
                ConsumerName = obj.ConsumerName,
                SubscriptionType = obj.SubscriptionType,
                InitialPosition = InitialPosition.Latest,
            });
        }

        private void XNodeEventService_MessageAcknowledged(Model.Events.Messages.MessageAcknowledgedArgs obj)
        {
            _consumerIOService.WriteMessageAcknowledged(obj);
        }


        #region Unacknowledge Messages
        private void XNodeEventService_ConsumerUnacknowledgedMessagesRequested(ConsumerConnectedArgs obj)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            if (_unacknowledgedMessageProcesses.ContainsKey(consumerKey))
                return;

            // We are adding the task to the Dictionary, when the task is done
            if (!_unacknowledgedMessageProcesses.ContainsKey(consumerKey))
                _unacknowledgedMessageProcesses.TryAdd(consumerKey, Task.Run(async () => await TransmitUnacknowledgedMessages(obj)));
        }

        private async Task TransmitUnacknowledgedMessages(ConsumerConnectedArgs obj)
        {
            int timeoutCounter = 0;
            while (File.Exists(ConsumerLocations.GetConsumerPointerFile(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName)) != true)
            {
                // try every second for 5 seconds if the db is created.
                timeoutCounter++;
                Thread.Sleep(1000);
                if (timeoutCounter == 5)
                    return;
            }

            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            ConsumerPointerContext consumerPointerContext = _consumerIOService.GetConsumerConnector(consumerKey).ConsumerPointerContext;
            try
            {
                consumerPointerContext = _consumerIOService.GetConsumerConnector(consumerKey).ConsumerPointerContext;
            }
            catch (Exception)
            {
                _logger.LogError($"Couldn't sent unacknoledge messages to consumer '{obj.ConsumerName}' at {consumerKey.Replace("~", "/")}");
                ReleaseUnacknoledgedMessageTasks(consumerKey);
                return;
            }

            List<MessageFile> partitionFiles = GetPartitionFiles(obj.Tenant, obj.Product, obj.Component, obj.Topic);
            bool isNewConsumer = false;

            try
            {
                // check if connection is open
                CheckPointerDbConnection(consumerPointerContext, consumerKey);

                var unackedMessages = consumerPointerContext.ConsumerMessages.Where(x => x.IsAcknowledged == false).OrderBy(x => x.SentDate).ToList();
                if (unackedMessages.Count == 0)
                {
                    int totalCount = consumerPointerContext.ConsumerMessages.Count();
                    if (totalCount == 0)
                    {
                        // Checking if this is a new consumer.
                        if (obj.InitialPosition == InitialPosition.Latest)
                            return;

                        unackedMessages = consumerPointerContext.ConsumerMessages.OrderBy(x => x.SentDate).ToList();
                        isNewConsumer = true;
                    }
                }

                _consumerConnectionRepository.GetConsumerById(consumerKey).StorageStateProperty.IsNewConsumer = isNewConsumer;
                await AnalysePartitionFiles(obj, partitionFiles, unackedMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldn't sent unacknoledge messages to consumer '{obj.ConsumerName}' at {consumerKey.Replace("~", "/")}; errorDetails = {ex.Message}");
            }

            ReleaseUnacknoledgedMessageTasks(consumerKey, true);
        }

        private void ReleaseUnacknoledgedMessageTasks(string consumerKey, bool forceRelease = true)
        {
            if (_unacknowledgedMessageProcesses.ContainsKey(consumerKey) != true)
                return;

            if (forceRelease == true)
            {
                _logger.LogWarning($"Unacknowledged message transmitter for '{consumerKey.Replace("~", "/")}' has been released");

                _consumerConnectionRepository.RemoveConsumer(consumerKey);
                _unacknowledgedMessageProcesses[consumerKey].Dispose();
                _unacknowledgedMessageProcesses.TryRemove(consumerKey, out _);
            }
        }

        private void CheckPointerDbConnection(ConsumerPointerContext tenantContext, string consumerKey)
        {
            int counter = 0;
            while (counter != 10)
            {
                if (tenantContext.Database.CanConnect())
                {
                    _logger.LogInformation($"Pointer database for '{consumerKey.Replace("~", "/")}' is responding");
                    break;
                }
                Thread.Sleep(1000);
                counter++;
                _logger.LogError($"Pointer database for {consumerKey.Replace("~", "/")} is not responding, trying to connect {counter} of 10");
            }
        }

        private async Task AnalysePartitionFiles(ConsumerConnectedArgs obj, List<MessageFile> partitionFiles, IEnumerable<Model.Entities.ConsumerMessage> unackedMessages)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            var isNewConsumer = _consumerConnectionRepository.GetConsumerById(consumerKey).StorageStateProperty.IsNewConsumer;
            foreach (var paritionFile in partitionFiles)
            {
                // here we do check partitions db messages....
                string topicName = _messageIOService.AddMessageFileConnectorGetKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, paritionFile.PartitionDate);
                var partitionContext = _messageIOService.GetPartitionMessageContext(topicName, paritionFile.PartitionDate);

                // Get all rows
                var rows = partitionContext.Messages.OrderBy(x => x.SentDate).ToList();

                // If is old consumer send only unacknowledged ones.
                if (isNewConsumer != true)
                {
                    if (unackedMessages.Count() == 0)
                        return;

                    rows = partitionContext.Messages
                        .Where(r => unackedMessages.Select(s => s.MessageId).Any(u => u == r.MessageId))
                        .OrderBy(x => x.SentDate)
                        .ToList();
                }

                await AnalyseFileRows(obj, rows, paritionFile.PartitionDate);

                // here is a bug #91, is removing all items form the list
                // for now we are removing this condition
                //if (isNewConsumer != true)
                //    unackedMessages.ToList().RemoveAll(r => rows.Any(u => u.MessageId == r.MessageId));

                // Remove from memory all not used data from here...
                GC.Collect();
                GC.SuppressFinalize(this);
                GC.SuppressFinalize(rows);
            }
        }

        private async Task AnalyseFileRows(ConsumerConnectedArgs obj, List<Model.Entities.Message> rows, DateTime partitionDate)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            var isNewConsumer = _consumerConnectionRepository.GetConsumerById(consumerKey).StorageStateProperty.IsNewConsumer;

            if (isNewConsumer == true)
                CachePointers(obj, rows, partitionDate);

            var consumerMessages = new List<ConsumerMessage>();
            foreach (var row in rows)
            {
                consumerMessages.Add(new ConsumerMessage()
                {
                    Consumer = obj.ConsumerName,
                    Message = new Message()
                    {
                        Tenant = obj.Tenant,
                        Product = obj.Product,
                        Component = obj.Component,
                        Topic = obj.Topic,
                        Id = row.MessageId,

                        SentDate = row.SentDate,

                        MessageRaw = row.Payload.JsonToObject<object>(),
                        Headers = row.Headers.JsonToObject<Dictionary<string, object>>()
                    }
                });

                if (_consumerConnectionRepository.GetConsumerById(consumerKey) != null)
                    await SendToNodes(consumerMessages);
            }
            if (_consumerConnectionRepository.GetConsumerById(consumerKey) != null)
                await SendToNodes(consumerMessages, true);

            consumerMessages.Clear();
        }

        private async Task SendToNodes(List<ConsumerMessage> consumerMessages, bool sendTheRest = false)
        {
            if (sendTheRest == false)
            {
                if (consumerMessages.Count == _partitionConfiguration.BatchSize)
                {
                    foreach (var xNode in _xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                    {
                        //Transmit messages to the other nodes.
                        await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessagesToConsumer", consumerMessages);
                    }
                    consumerMessages.Clear();
                }
            }
            else
            {
                if (consumerMessages.Count > 0)
                {
                    foreach (var xNode in _xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                    {
                        //Transmit messages to the other nodes.
                        await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessagesToConsumer", consumerMessages);
                    }
                }
            }
        }

        private void CachePointers(ConsumerConnectedArgs obj, List<Model.Entities.Message> rows, DateTime partitionDate)
        {
            // Unacknowledge message, add to the pointer, and send
            _logger.LogInformation($"Pointers are caching for {obj.ConsumerName} at {obj.Tenant}/{obj.Product}/{obj.Component}/{obj.Topic}");
            for (int i = 0; i < rows.Count; i++)
            {
                _consumerIOService.WriteMessageAcknowledged(new Model.Events.Messages.MessageAcknowledgedArgs()
                {
                    Tenant = obj.Tenant,
                    Product = obj.Product,
                    Component = obj.Component,
                    Topic = obj.Topic,
                    Consumer = obj.ConsumerName,
                    IsAcknowledged = false,
                    MessageId = rows[i].MessageId,
                },
                partitionDate.ToString("yyyy-MM-dd"));
            }
        }

        private List<MessageFile> GetPartitionFiles(string tenant, string product, string component, string topic)
        {
            List<MessageFile> messages = new List<MessageFile>();
            string[] partitions = Directory
               .GetFiles(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic), "*.xandy");

            Array.ForEach(partitions, partition =>
            {
                string fileName = Path.GetFileNameWithoutExtension(partition);
                string[] partitionNameSplited = fileName.Split("_");

                var partitionDate = DateTime.ParseExact($"{partitionNameSplited[2]}-{partitionNameSplited[3]}-{partitionNameSplited[4]} {partitionNameSplited[5]}:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                messages.Add(new MessageFile() { Path = partition, PartitionDate = partitionDate });
            });

            List<MessageFile> sorted = messages.OrderBy(m => m.PartitionDate).ToList();

            return sorted;
        }
        #endregion


        private async Task NotifyNodesForConsumerConnection(NotifyConsumerConnection obj)
        {
            if (obj.SubscriptionType != SubscriptionType.Shared)
            {
                _logger.LogInformation($"Notify other nodes for Consumer '{obj.ConsumerName}' connection status");
                // Transmit the message to other connected XNODES.
                if (_xNodeEventService.GetXNodeConnectionRepository().GetAllServices().Count > 1)
                {
                    foreach (var xNode in _xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                    {
                        // this node should be ignored because, it already produces the messages to consumers connected.
                        if (xNode.Key != _xNodeEventService.GetCurrentXNodeServiceUrl())
                        {
                            //Transmit the message to the other nodes.
                            await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("NotifyNodesForConsumerConnection", obj);
                        }
                    }
                }
            }
        }


        private string GenerateConsumerKey(string tenant, string product, string component, string topic, string consumer)
        {
            return $"{tenant}~{product}~{component}~{topic}~{consumer}";
        }
    }
}
