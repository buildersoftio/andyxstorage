using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.Logging;

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
                tenantIOService.WriteAgentStateInTenantLog(tenant.Key, obj.AgentId.ToString(), "DISCONNECTED");
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
                tenantIOService.WriteAgentStateInTenantLog(tenant.Key, obj.AgentId.ToString(), "CONNECTED");
            }

        }
    }
}
