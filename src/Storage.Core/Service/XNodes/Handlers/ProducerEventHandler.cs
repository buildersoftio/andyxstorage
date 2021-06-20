using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.Logging;
using System;

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
        private void XNodeEventService_ProducerConnected(Model.Events.Producers.ProducerConnectedArgs obj)
        {
            producerIOService.TryCreateProducerDirectory(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Producers.Producer()
            {
                Id = obj.Id,
                Name = obj.ProducerName,
                CreatedDate = DateTime.Now
            });
        }

        private void XNodeEventService_ProducerDisconnected(Model.Events.Producers.ProducerDisconnectedArgs obj)
        {
            producerIOService.WriteDisconnectedProducerLog(obj.Tenant, obj.Product, obj.Component, obj.Topic, new Model.App.Producers.Producer()
            {
                Id = obj.Id,
                Name = obj.ProducerName,
                CreatedDate = DateTime.Now
            });
        }
    }
}
