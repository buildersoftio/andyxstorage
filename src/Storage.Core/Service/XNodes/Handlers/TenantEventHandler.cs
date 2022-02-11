using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Buildersoft.Andy.X.Storage.Core.Service.XNodes.Handlers
{
    public class TenantEventHandler
    {
        private readonly ILogger<SystemService> logger;
        private readonly XNodeEventService xNodeEventService;
        private readonly TenantIOService tenantIOService;

        public TenantEventHandler(ILogger<SystemService> logger, XNodeEventService xNodeEventService, TenantIOService tenantIOService)
        {
            this.logger = logger;
            this.xNodeEventService = xNodeEventService;
            this.tenantIOService = tenantIOService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            xNodeEventService.TenantCreated += XNodeEventService_TenantCreated;
            xNodeEventService.TenantUpdated += XNodeEventService_TenantUpdated;

            xNodeEventService.TenantTokenCreated += XNodeEventService_TenantTokenCreated;
            xNodeEventService.TenantTokenRevoked += XNodeEventService_TenantTokenRevoked;

            xNodeEventService.ProductCreated += XNodeEventService_ProductCreated;
            xNodeEventService.ProductUpdated += XNodeEventService_ProductUpdated;

            xNodeEventService.ComponentCreated += XNodeEventService_ComponentCreated;
            xNodeEventService.ComponentUpdated += XNodeEventService_ComponentUpdated;

            xNodeEventService.ComponentTokenCreated += XNodeEventService_ComponentTokenCreated;
            xNodeEventService.ComponentTokenRevoked += XNodeEventService_ComponentTokenRevoked;

            xNodeEventService.TopicCreated += XNodeEventService_TopicCreated;
            xNodeEventService.TopicUpdated += XNodeEventService_TopicUpdated;

        }

        private void XNodeEventService_TenantCreated(Model.Events.Tenants.TenantCreatedArgs obj)
        {
            tenantIOService.TryCreateTenantDirectory(obj.Name, new Model.App.Tenants.Tenant() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
            logger.LogInformation($"Tenant '{obj.Name}' properties created");
        }

        private void XNodeEventService_TenantUpdated(Model.Events.Tenants.TenantUpdatedArgs obj)
        {
            tenantIOService.TryUpdateTenantDirectory(obj.Name, new Model.App.Tenants.Tenant() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
            logger.LogInformation($"Tenant '{obj.Name}' settings updated");
        }

        private async void XNodeEventService_TenantTokenCreated(Model.Events.Tenants.TenantTokenCreatedArgs obj)
        {
            foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
            {
                // This node should be ignored because, it already produces CreateTenantToken.
                if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                {
                    // Transmit the CreateTenantToken to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("CreateTenantToken", obj);
                }
            }

            logger.LogInformation($"TenantToken created for '{obj.Tenant}', settings updated");
        }

        private async void XNodeEventService_TenantTokenRevoked(Model.Events.Tenants.TenantTokenRevokedArgs obj)
        {
            foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
            {
                // This node should be ignored because, it already produces RevokeTenantToken.
                if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                {
                    // Transmit the RevokeTenantToken to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("RevokeTenantToken", obj);
                }
            }

            logger.LogInformation($"TenantToken revoked for '{obj.Tenant}', settings updated");
        }

        private void XNodeEventService_ProductCreated(Model.Events.Products.ProductCreatedArgs obj)
        {
            tenantIOService.TryCreateProductDirectory(obj.Tenant, new Model.App.Products.Product() { Id = obj.Id, Name = obj.Name });
            logger.LogInformation($"Product '{obj.Name}' properties at {obj.Tenant} created");
        }

        private void XNodeEventService_ProductUpdated(Model.Events.Products.ProductUpdatedArgs obj)
        {
            tenantIOService.TryCreateProductDirectory(obj.Tenant, new Model.App.Products.Product() { Id = obj.Id, Name = obj.Name });
        }

        private void XNodeEventService_ComponentCreated(Model.Events.Components.ComponentCreatedArgs obj)
        {
            tenantIOService.TryCreateComponentDirectory(obj.Tenant, obj.Product, new Model.App.Components.Component() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
            logger.LogInformation($"Component '{obj.Name}' properties at {obj.Tenant}/{obj.Product} created");
        }

        private void XNodeEventService_ComponentUpdated(Model.Events.Components.ComponentUpdatedArgs obj)
        {
            tenantIOService.TryCreateComponentDirectory(obj.Tenant, obj.Product, new Model.App.Components.Component() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
        }

        private async void XNodeEventService_ComponentTokenCreated(Model.Events.Components.ComponentTokenCreatedArgs obj)
        {
            foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
            {
                // This node should be ignored because, it already produces CreateComponentToken.
                if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                {
                    // Transmit the CreateComponentToken to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("CreateComponentToken", obj);
                }
            }

            logger.LogInformation($"ComponentToken created for '{obj.Tenant}/{obj.Product}/{obj.Component}', settings updated");
        }
        private async void XNodeEventService_ComponentTokenRevoked(Model.Events.Components.ComponentTokenRevokedArgs obj)
        {
            foreach (var xNode in xNodeEventService.GetXNodeConnectionRepository().GetAllServices())
            {
                // This node should be ignored because, it already produces RevokeComponentToken
                if (xNode.Key != xNodeEventService.GetCurrentXNodeServiceUrl())
                {
                    // Transmit the RevokeComponentToken to the other nodes.
                    await xNode.Value.Values.ToList()[0].GetHubConnection().SendAsync("RevokeComponentToken", obj);
                }
            }

            logger.LogInformation($"ComponentToken revoked for '{obj.Tenant}/{obj.Product}/{obj.Component}', settings updated");
        }

        private void XNodeEventService_TopicCreated(Model.Events.Topics.TopicCreatedArgs obj)
        {
            tenantIOService.TryCreateTopicDirectory(obj.Tenant, obj.Product, obj.Component, new Model.App.Topics.Topic() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
            logger.LogInformation($"Topic '{obj.Name}' properties at {obj.Tenant}/{obj.Product}/{obj.Component} created");
        }

        private void XNodeEventService_TopicUpdated(Model.Events.Topics.TopicUpdatedArgs obj)
        {
            tenantIOService.TryUpdateTopicDirectory(obj.Tenant, obj.Product, obj.Component, new Model.App.Topics.Topic() { Id = obj.Id, Name = obj.Name, Settings = obj.Settings });
        }
    }
}
