using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

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

        private void XNodeEventService_ConsumerUnacknowledgedMessagesRequested(Model.Events.Consumers.ConsumerConnectedArgs obj)
        {
            if (File.Exists(ConsumerLocations.GetConsumerPointerFile(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName)) != true)
                return;

            TenantContext tenantContext = new TenantContext(ConsumerLocations.GetConsumerPointerFile(obj.Tenant, obj.Product, obj.Component, obj.Topic, obj.ConsumerName));
            tenantContext.ConsumerMessages.Where(x => x.IsAcknowledged == false).ForEachAsync(async message =>
            {
                MessageRow msgRow = consumerIOService.ReadMessageRow(obj.Tenant, obj.Product, obj.Component, obj.Topic, message.MessageId);

                // If there is now message registered with this ID, just ignore it. The message details has not arrived jet.
                if (msgRow == null)
                    return;

                var consumerMessage = new ConsumerMessage()
                {
                    Consumer = obj.ConsumerName,
                    Message = new Message()
                    {
                        Tenant = obj.Tenant,
                        Product = obj.Product,
                        Component = obj.Component,
                        Topic = obj.Topic,
                        Id = message.MessageId,
                        MessageRaw = msgRow.MessageRaw
                    }
                };

                foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                {
                    //Transmit the message to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessagesToConsumer", consumerMessage);
                }
            });
        }
    }
}
