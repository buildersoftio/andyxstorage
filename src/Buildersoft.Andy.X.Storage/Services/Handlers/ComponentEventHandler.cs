using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class ComponentEventHandler
    {
        private readonly SignalRDataStorageService _service;
        private readonly IComponentService _componentService;

        public ComponentEventHandler(SignalRDataStorageService service, IComponentService componentService)
        {
            _service = service;
            _componentService = componentService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.ComponentCreated += DataStorageService_ComponentCreated;
            _service.ComponentRead += DataStorageService_ComponentRead;
            _service.ComponentUpdated += DataStorageService_ComponentUpdated;
            _service.ComponentDeleted += DataStorageService_ComponentDeleted;
        }

        private void DataStorageService_ComponentDeleted(Data.Model.Events.Components.ComponentDeletedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ComponentUpdated(Data.Model.Events.Components.ComponentUpdatedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ComponentRead(Data.Model.Events.Components.ComponentReadArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_ComponentCreated(Data.Model.Events.Components.ComponentCreatedArgs obj)
        {
            _componentService.CreateComponent(obj);
        }
    }
}
