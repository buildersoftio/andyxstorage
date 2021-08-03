using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;

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

        private async void XNodeEventService_ConsumerUnacknowledgedMessagesRequested(Model.Events.Consumers.ConsumerConnectedArgs obj)
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

            TenantContext tenantContext = new TenantContext(ConsumerLocations.GetConsumerPointerFile(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName));
            string[] paritionFiles = Directory.GetFiles(TenantLocations.GetMessageRootDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic));
            bool isNewConsumer = false;

            var unackedMessages = tenantContext.ConsumerMessages.Where(x => x.IsAcknowledged == false).OrderBy(x => x.SentDate).ToList();
            if (unackedMessages.Count == 0)
            {
                int totalCount = tenantContext.ConsumerMessages.Count();
                if (totalCount == 0)
                {
                    unackedMessages = tenantContext.ConsumerMessages.OrderBy(x => x.SentDate).ToList();
                    isNewConsumer = true;
                }
            }

            foreach (string paritionFile in paritionFiles)
            {
                string[] lines = consumerIOService.TryReadAllLines(paritionFile);
                if (lines == null)
                    continue;

                // Get all rows
                var rows = lines.Select(line => line.JsonToObjectAndDecrypt<MessageRow>()).ToList();

                // If is old consumer send only unacknowledged ones.
                if (isNewConsumer != true)
                    rows = lines.Select(line => line.JsonToObjectAndDecrypt<MessageRow>()).Where(r => unackedMessages.Any(u => u.MessageId == r.Id)).ToList();

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

                if (isNewConsumer != true)
                    unackedMessages.RemoveAll(r => rows.Any(u => u.Id == r.MessageId));
            }
        }
    }
}
