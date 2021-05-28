﻿using Buildersoft.Andy.X.Storage.Core.Service.System;
using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.Logging;

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

            xNodeEventService.ProductCreated += XNodeEventService_ProductCreated;
            xNodeEventService.ProductUpdated += XNodeEventService_ProductUpdated;

            xNodeEventService.ComponentCreated += XNodeEventService_ComponentCreated;
            xNodeEventService.ComponentUpdated += XNodeEventService_ComponentUpdated;

            xNodeEventService.TopicCreated += XNodeEventService_TopicCreated;
        }

        private void XNodeEventService_TenantCreated(Model.Events.Tenants.TenantCreatedArgs obj)
        {
            tenantIOService.TryCreateTenantDirectory(obj.Name, new Model.App.Tenants.Tenant() { Id = obj.Id, Name = obj.Name, DigitalSignature = obj.DigitalSignature });
        }

        private void XNodeEventService_TenantUpdated(Model.Events.Tenants.TenantUpdatedArgs obj)
        {
            // TODO: Implement later
        }

        private void XNodeEventService_ProductCreated(Model.Events.Products.ProductCreatedArgs obj)
        {
            tenantIOService.TryCreateProductDirectory(obj.Tenant, new Model.App.Products.Product() { Id = obj.Id, Name = obj.Name });
        }

        private void XNodeEventService_ProductUpdated(Model.Events.Products.ProductUpdatedArgs obj)
        {
            // TODO: Implement later
        }

        private void XNodeEventService_ComponentCreated(Model.Events.Components.ComponentCreatedArgs obj)
        {
            tenantIOService.TryCreateComponentDirectory(obj.Tenant, obj.Product, new Model.App.Components.Component() { Id = obj.Id, Name = obj.Name });
        }

        private void XNodeEventService_ComponentUpdated(Model.Events.Components.ComponentUpdatedArgs obj)
        {
            // TODO: Implement later
        }

        private void XNodeEventService_TopicCreated(Model.Events.Topics.TopicCreatedArgs obj)
        {
            tenantIOService.TryCreateTopicDirectory(obj.Tenant, obj.Product, obj.Component, new Model.App.Topics.Topic() { Id = obj.Id, Name = obj.Name, Schema = obj.Schema });
        }
    }
}