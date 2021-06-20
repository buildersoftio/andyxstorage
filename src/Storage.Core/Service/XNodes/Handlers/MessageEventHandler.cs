using Buildersoft.Andy.X.Storage.Core.Service.System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class MessageEventHandler
    {
        private readonly ILogger<SystemService> logger;
        private readonly XNodeEventService xNodeEventService;

        public MessageEventHandler(ILogger<SystemService> logger, XNodeEventService xNodeEventService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.MessageStored += XNodeEventService_MessageStored;
        }

        private async void XNodeEventService_MessageStored(Model.Events.Messages.MessageStoredArgs obj)
        {
            // TODO Store the message in this storage.
            //
            //
            //

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
