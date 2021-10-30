using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class ConsumerEventHandler
    {
        private readonly ILogger<SystemService> logger;
        private readonly XNodeEventService xNodeEventService;
        private readonly ConsumerIOService consumerIOService;

        public ConsumerEventHandler(ILogger<SystemService> logger,
            XNodeEventService xNodeEventService,
            ConsumerIOService consumerIOService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;
            this.consumerIOService = consumerIOService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.ConsumerConnected += XNodeEventService_ConsumerConnected;
            xNodeEventService.ConsumerDisconnected += XNodeEventService_ConsumerDisconnected;
            xNodeEventService.ConsumerUnacknowledgedMessagesRequested += XNodeEventService_ConsumerUnacknowledgedMessagesRequested;
            xNodeEventService.MessageAcknowledged += XNodeEventService_MessageAcknowledged;
        }

        private void XNodeEventService_ConsumerConnected(Model.Events.Consumers.ConsumerConnectedArgs obj)
        {
            consumerIOService.TryCreateConsumerDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Consumers.Consumer()
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

        private void XNodeEventService_ConsumerDisconnected(Model.Events.Consumers.ConsumerDisconnectedArgs obj)
        {
            consumerIOService.WriteDisconnectedConsumerLog(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Consumers.Consumer()
            {
                Id = obj.Id,
                Name = obj.ConsumerName,
                Tenant = obj.Tenant,
                Product = obj.Product,
                Component = obj.Component,
                Topic = obj.Topic,
                CreatedDate = DateTime.Now
            });
        }

        private void XNodeEventService_MessageAcknowledged(Model.Events.Messages.MessageAcknowledgedArgs obj)
        {
            consumerIOService.WriteMessageAcknowledged(obj);
        }

        private async void XNodeEventService_ConsumerUnacknowledgedMessagesRequested(ConsumerConnectedArgs obj)
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

            string consumerKey = $"{obj.Tenant}~{obj.Product}~{obj.Component}~{obj.Topic}~{obj.ConsumerName}";
            TenantContext tenantContext = consumerIOService.GetConsumerConnector(consumerKey).TenantContext;
            try
            {
                tenantContext = consumerIOService.GetConsumerConnector(consumerKey).TenantContext;
            }
            catch (Exception)
            {
                logger.LogError($"Couldn't sent unacknoledge messages to consumer '{obj.ConsumerName}' at {consumerKey}");
                return;
            }

            List<MessageFile> partitionFiles = GetPartitionFiles(obj.Tenant, obj.Product, obj.Component, obj.Topic);

            bool isNewConsumer = false;

            try
            {
                // check if connection is open
                CheckPointerDbConnection(tenantContext, consumerKey);

                var unackedMessages = tenantContext.ConsumerMessages.Where(x => x.IsAcknowledged == false).OrderBy(x => x.SentDate).ToList();
                if (unackedMessages.Count == 0)
                {
                    int totalCount = tenantContext.ConsumerMessages.Count();
                    if (totalCount == 0)
                    {
                        // Checking if this is a new consumer.
                        if (obj.InitialPosition == InitialPosition.Latest)
                            return;

                        unackedMessages = tenantContext.ConsumerMessages.OrderBy(x => x.SentDate).ToList();
                        isNewConsumer = true;
                    }
                }

                await AnalysePartitionFiles(obj, partitionFiles, isNewConsumer, unackedMessages);
            }
            catch (Exception ex)
            {
                logger.LogError($"Couldn't sent unacknoledge messages to consumer '{obj.ConsumerName}' at {consumerKey}; errorDetails = {ex.Message}");
            }
        }

        private void CheckPointerDbConnection(TenantContext tenantContext, string consumerKey)
        {
            int counter = 0;
            while (counter != 10)
            {
                if (tenantContext.Database.CanConnect())
                {
                    logger.LogInformation($"Pointer database for '{consumerKey}' is responding");
                    break;
                }
                Thread.Sleep(1000);
                counter++;
                logger.LogError($"Pointer database for {consumerKey} is not responding, trying to connect {counter} of 10");
            }
        }

        private async Task AnalysePartitionFiles(ConsumerConnectedArgs obj, List<MessageFile> partitionFiles, bool isNewConsumer, List<Model.Entities.ConsumerMessage> unackedMessages)
        {
            foreach (var paritionFile in partitionFiles)
            {
                string[] lines = FileReader.TryReadAllLines(paritionFile.Path);
                if (lines == null)
                    continue;

                // Get all rows
                var rows = lines.Select(line => line.JsonToObjectAndDecrypt<MessageRow>()).ToList();

                // If is old consumer send only unacknowledged ones.
                if (isNewConsumer != true)
                    rows = lines.Select(line => line.JsonToObjectAndDecrypt<MessageRow>())
                        .Where(r => unackedMessages.Any(u => u.MessageId == r.Id))
                        .ToList();

                await AnalyseFileRows(obj, rows);

                if (isNewConsumer != true)
                    unackedMessages.RemoveAll(r => rows.Any(u => u.Id == r.MessageId));
            }
        }

        private async Task AnalyseFileRows(ConsumerConnectedArgs obj, List<MessageRow> rows)
        {
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
                        Id = row.Id,
                        MessageRaw = row.MessageRaw
                    }
                };

                foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                {
                    //Transmit the message to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessagesToConsumer", consumerMessage);
                }
            }
        }

        private List<MessageFile> GetPartitionFiles(string tenant, string product, string component, string topic)
        {
            List<MessageFile> messages = new List<MessageFile>();
            string[] partitions = Directory
               .GetFiles(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic));

            Array.ForEach(partitions, partition =>
            {
                string fileName = Path.GetFileNameWithoutExtension(partition);
                int partitionIndex = Convert.ToInt32(fileName.Split('_').Last());

                messages.Add(new MessageFile() { Path = partition, PartitionIndex = partitionIndex });
            });

            List<MessageFile> sorted = messages.OrderBy(m => m.PartitionIndex).ToList();

            return sorted;
        }
    }
}
