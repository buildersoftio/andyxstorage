using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class AgentEventHandler
    {
        private readonly XNodeEventService xNodeEventService;
        private readonly TenantIOService tenantIOService;
        private readonly ILogger<SystemService> logger;

        public AgentEventHandler(ILogger<SystemService> logger, XNodeEventService xNodeEventService, TenantIOService tenantIOService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;
            this.tenantIOService = tenantIOService;
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.StorageConnected += XNodeEventService_StorageConnected;
            xNodeEventService.StorageDisconnected += XNodeEventService_StorageDisconnected;
        }

        private void XNodeEventService_StorageDisconnected(Model.Events.Agents.AgentDisconnectedArgs obj)
        {
            logger.LogInformation($"Agent '{obj.Agent}' with id '{obj.AgentId}' is disconnected");

            foreach (var tenant in obj.Tenants)
            {
                tenantIOService.WriteAgentStateInTenantLog(tenant.Key, obj.AgentId.ToString(), "disconnected");
            }
        }

        private void XNodeEventService_StorageConnected(Model.Events.Agents.AgentConnectedArgs obj)
        {
            logger.LogInformation($"Agent '{obj.Agent}' with id '{obj.AgentId}' is connected");

            foreach (var tenant in obj.Tenants)
            {
                logger.LogInformation($"Agent '{obj.AgentId}' is connected to '{tenant.Key}' tenant");

                // Trying to create new tenants locations in the storage.
                tenantIOService.TryCreateTenantDirectory(tenant.Key, tenant.Value);
                tenantIOService.WriteAgentStateInTenantLog(tenant.Key, obj.AgentId.ToString(), "connected");

                tenant.Value.Products.ToList().ForEach(product =>
                {
                    tenantIOService.TryCreateProductDirectory(tenant.Key, product.Value);

                    product.Value.Components.ToList().ForEach(component =>
                    {
                        tenantIOService.TryCreateComponentDirectory(tenant.Key, product.Key, component.Value);
                        component.Value.Topics.ToList().ForEach(topic =>
                        {
                            tenantIOService.TryCreateTopicDirectory(tenant.Key, product.Key, component.Key, topic.Value);
                        });
                    });
                });
            }
        }
    }
}
