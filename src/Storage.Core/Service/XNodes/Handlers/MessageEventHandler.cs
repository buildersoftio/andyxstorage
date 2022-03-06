using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.Events.Messages;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class MessageEventHandler
    {
        private readonly ILogger<SystemService> logger;
        private readonly XNodeEventService xNodeEventService;
        private readonly MessageIOService messageIOService;

        public MessageEventHandler(ILogger<SystemService> logger,
            XNodeEventService xNodeEventService,
            MessageIOService messageIOService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;
            this.messageIOService = messageIOService;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.MessageStored += XNodeEventService_MessageStored;
            xNodeEventService.MessagesStored += XNodeEventService_MessagesStored;
        }

        private async void XNodeEventService_MessagesStored(List<MessageStoredArgs> obj)
        {
            foreach (var message in obj)
            {
                messageIOService.StoreMessage(new Model.App.Messages.Message()
                {
                    Tenant = message.Tenant,
                    Id = message.Id,
                    Component = message.Component,
                    MessageRaw = message.MessageRaw,
                    Headers = message.Headers,
                    Product = message.Product,
                    Topic = message.Topic,
                    SentDate = message.SentDate
                });

                await RetransmitMessageToOtherNodes(message);
            }
        }

        private async void XNodeEventService_MessageStored(MessageStoredArgs obj)
        {
            messageIOService.StoreMessage(new Model.App.Messages.Message()
            {
                Tenant = obj.Tenant,
                Id = obj.Id,
                Component = obj.Component,
                MessageRaw = obj.MessageRaw,
                Headers = obj.Headers,
                Product = obj.Product,
                Topic = obj.Topic,
                SentDate = obj.SentDate
            });

            await RetransmitMessageToOtherNodes(obj);
        }

        private async Task RetransmitMessageToOtherNodes(MessageStoredArgs obj)
        {
            // Transmit the message to other connected XNODES.
            if (xNodeEventService.GetXNodeConnectionRepository().GetAllServices().Count > 1)
            {
                foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                {
                    // this node should be ignored because, it already produces the messages to consumers connected.
                    if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                    {
                        //Transmit the message to the other nodes.
                        await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("TransmitMessageToThisNodeConsumers", obj);
                    }
                }
            }
        }
    }
}
