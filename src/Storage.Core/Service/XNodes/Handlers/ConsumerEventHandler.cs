using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Model.Events.Consumers;
using Buildersoft.Andy.X.Storage.Model.Files;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<string, Task> _unacknowledgedMessageProcesses;

        public ConsumerEventHandler(
            ILogger<SystemService> logger,
            XNodeEventService xNodeEventService,
            ConsumerIOService consumerIOService,
            MessageIOService messageIOService)
        {
            _logger = logger;
            _xNodeEventService = xNodeEventService;
            _consumerIOService = consumerIOService;
            _messageIOService = messageIOService;
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

        private void XNodeEventService_ConsumerConnected(ConsumerConnectedArgs obj)
        {
            _consumerIOService.TryCreateConsumerDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Consumer()
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
            });
        }

        private void XNodeEventService_ConsumerDisconnected(ConsumerDisconnectedArgs obj)
        {
            _consumerIOService.WriteDisconnectedConsumerLog(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Consumer()
            {
                Id = obj.Id,
                Name = obj.ConsumerName,
                Tenant = obj.Tenant,
                Product = obj.Product,
                Component = obj.Component,
                Topic = obj.Topic,
                CreatedDate = DateTime.Now,
                SubscriptionType = obj.SubscriptionType,
            });
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);

            if (obj.SubscriptionType != SubscriptionType.Shared)
                ReleaseUnacknoledgedMessageTasks(consumerKey);
        }

        private void XNodeEventService_MessageAcknowledged(Model.Events.Messages.MessageAcknowledgedArgs obj)
        {
            _consumerIOService.WriteMessageAcknowledged(obj);
        }

        private void XNodeEventService_ConsumerUnacknowledgedMessagesRequested(ConsumerConnectedArgs obj)
        {
            string consumerKey = GenerateConsumerKey(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName);
            if (_unacknowledgedMessageProcesses.ContainsKey(consumerKey))
                return;

            // We are adding the task to the Dictionary, when the task is done
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

                await AnalysePartitionFiles(obj, partitionFiles, isNewConsumer, unackedMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Couldn't sent unacknoledge messages to consumer '{obj.ConsumerName}' at {consumerKey.Replace("~", "/")}; errorDetails = {ex.Message}");
            }

            ReleaseUnacknoledgedMessageTasks(consumerKey);
        }

        private void ReleaseUnacknoledgedMessageTasks(string consumerKey)
        {
            if (_unacknowledgedMessageProcesses.ContainsKey(consumerKey) != true)
                return;

            _logger.LogWarning($"Unacknowledged message transmitter for '{consumerKey.Replace("~", "/")}' has been released");

            _unacknowledgedMessageProcesses[consumerKey].Dispose();
            _unacknowledgedMessageProcesses.TryRemove(consumerKey, out _);
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

        private async Task AnalysePartitionFiles(ConsumerConnectedArgs obj, List<MessageFile> partitionFiles, bool isNewConsumer, IEnumerable<Model.Entities.ConsumerMessage> unackedMessages)
        {
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

                await AnalyseFileRows(obj, rows, paritionFile.PartitionDate, isNewConsumer);

                if (isNewConsumer != true)
                    unackedMessages.ToList().RemoveAll(r => rows.Any(u => u.MessageId == r.MessageId));
            }
        }

        private async Task AnalyseFileRows(ConsumerConnectedArgs obj, List<Model.Entities.Message> rows, DateTime partitionDate, bool isNewConsumer = false)
        {
            if (isNewConsumer == true)
                CachePointers(obj, rows, partitionDate);

            foreach (var row in rows)
            {
                var consumerMessage = new ConsumerMessage()
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
                };

                foreach (var xNode in _xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                {
                    //Transmit the message to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessagesToConsumer", consumerMessage);
                }
            }
        }

        private void CachePointers(ConsumerConnectedArgs obj, List<Model.Entities.Message> rows, DateTime partitionDate)
        {
            // Unacknowledge message, add to the pointer, and send
            _logger.LogInformation($"Pointers are caching for {obj.ConsumerName}");
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

                var partitionDate = DateTime.Parse($"{partitionNameSplited[2]}-{partitionNameSplited[3]}-{partitionNameSplited[4]}");

                messages.Add(new MessageFile() { Path = partition, PartitionDate = partitionDate });
            });

            List<MessageFile> sorted = messages.OrderBy(m => m.PartitionDate).ToList();

            return sorted;
        }

        private string GenerateConsumerKey(string tenant, string product, string component, string topic, string consumer)
        {
            return $"{tenant}~{product}~{component}~{topic}~{consumer}";
        }
    }
}
