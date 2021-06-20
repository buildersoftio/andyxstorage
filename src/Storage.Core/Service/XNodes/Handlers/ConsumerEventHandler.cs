using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.Logging;
using System;

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
    }
}
