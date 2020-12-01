using Buildersoft.Andy.X.Storage.Data.Model.Events.Components;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class ComponentService : IComponentService
    {
        private readonly ILogger<ComponentService> _logger;

        private ComponentRepository _componentRepository;
        public ComponentService(ILogger<ComponentService> logger)
        {
            _logger = logger;
        }
        public void CreateComponent(ComponentCreatedArgs componentCreatedArgs)
        {
            _componentRepository = new ComponentRepository(componentCreatedArgs.TenantName, componentCreatedArgs.ProductName);

            Data.Model.Components.Component component;
            if (_componentRepository.GetAll().ContainsKey(componentCreatedArgs.ComponentName) == true)
                throw new Exception($"There is a component registered with this name andyx://{componentCreatedArgs.TenantName}/{componentCreatedArgs.ProductName}/{componentCreatedArgs.ComponentName}");

            string componentLocation = TenantConfigFile.CreateComponentLocation(componentCreatedArgs.TenantName, componentCreatedArgs.ProductName, componentCreatedArgs.ComponentName);

            component = new Data.Model.Components.Component()
            {
                Id = componentCreatedArgs.ComponentId,
                Name = componentCreatedArgs.ComponentName,
                Location = componentLocation
            };

            _componentRepository.Add(componentCreatedArgs.ComponentName, component);
            _logger.LogInformation($"{componentCreatedArgs.TenantName}/{componentCreatedArgs.ProductName}/{componentCreatedArgs.ComponentName}: stored");
        }

        public bool DeleteComponent(ComponentDeletedArgs componentDeletedArgs)
        {
            throw new NotImplementedException();
        }

        public Data.Model.Components.Component ReadComponent(ComponentReadArgs componentReadArgs)
        {
            throw new NotImplementedException();
        }

        public ConcurrentDictionary<string, Data.Model.Components.Component> ReadComponents()
        {
            throw new NotImplementedException();
        }

        public Data.Model.Components.Component UpdateComponent(ComponentUpdatedArgs componentDeletedArgs)
        {
            throw new NotImplementedException();
        }

        System.ComponentModel.Component IComponentService.ReadComponent(ComponentReadArgs componentReadArgs)
        {
            throw new NotImplementedException();
        }

        ConcurrentDictionary<string, System.ComponentModel.Component> IComponentService.ReadComponents()
        {
            throw new NotImplementedException();
        }

        System.ComponentModel.Component IComponentService.UpdateComponent(ComponentUpdatedArgs componentDeletedArgs)
        {
            throw new NotImplementedException();
        }
    }
}
