using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.Commands.Producer;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class ProducerEventHandler
    {
        private readonly ILogger<SystemService> logger;
        private readonly XNodeEventService xNodeEventService;
        private readonly ProducerIOService producerIOService;

        public ProducerEventHandler(ILogger<SystemService> logger, XNodeEventService xNodeEventService, ProducerIOService producerIOService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;
            this.producerIOService = producerIOService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.ProducerConnected += XNodeEventService_ProducerConnected;
            xNodeEventService.ProducerDisconnected += XNodeEventService_ProducerDisconnected;
        }
        private async void XNodeEventService_ProducerConnected(Model.Events.Producers.ProducerConnectedArgs obj)
        {
            producerIOService.TryCreateProducerDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Producers.Producer()
            {
                Id = obj.Id,
                Name = obj.ProducerName,
                CreatedDate = DateTime.Now
            });

            // connect
            await NotifyNodesForProducerConnection(new NotifyProducerConnection()
            {
                ConnectionType = Model.Commands.Consumer.ConnectionType.Connected,

                Id = obj.Id,
                ProducerName = obj.ProducerName,
                Component = obj.Component,
                Topic = obj.Topic,
                Product = obj.Product,
                Tenant = obj.Tenant
            });
        }

        private async void XNodeEventService_ProducerDisconnected(Model.Events.Producers.ProducerDisconnectedArgs obj)
        {
            producerIOService.WriteDisconnectedProducerLog(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Producers.Producer()
            {
                Id = obj.Id,
                Name = obj.ProducerName,
                CreatedDate = DateTime.Now
            });

            // disconnect
            await NotifyNodesForProducerConnection(new NotifyProducerConnection()
            {
                ConnectionType = Model.Commands.Consumer.ConnectionType.Disconnected,

                Id = obj.Id,
                ProducerName = obj.ProducerName,
                Component = obj.Component,
                Topic = obj.Topic,
                Product = obj.Product,
                Tenant = obj.Tenant
            });
        }


        private async Task NotifyNodesForProducerConnection(NotifyProducerConnection obj)
        {

            logger.LogInformation($"Notify other nodes for Producer '{obj.ProducerName}' connection status");
            // Transmit the message to other connected XNODES.
            if (xNodeEventService.GetXNodeConnectionRepository().GetAllServices().Count > 1)
            {
                foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
                {
                    // this node should be ignored because, it already produces the messages to consumers connected.
                    if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                    {
                        //Transmit the message to the other nodes.
                        await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("NotifyNodesForProducerConnection", obj);
                    }
                }
            }

        }
    }
}
