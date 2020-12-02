using Buildersoft.Andy.X.Storage.Logic.Services;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class TenantEventHandler
    {
        private readonly NodeDataStorageService _service;
        private readonly ITenantService _tenantService;
        public TenantEventHandler(NodeDataStorageService service, ITenantService tenantService)
        {
            _service = service;
            _tenantService = tenantService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.TenantCreated += DataStorageService_TenantCreated;
            _service.TenantRead += DataStorageService_TenantRead;
            _service.TenantUpdated += DataStorageService_TenantUpdated;
            _service.TenantDeleted += DataStorageService_TenantDeleted;
        }

        private void DataStorageService_TenantDeleted(Data.Model.Events.Tenants.TenantDeletedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_TenantUpdated(Data.Model.Events.Tenants.TenantUpdatedArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_TenantRead(Data.Model.Events.Tenants.TenantReadArgs obj)
        {
            throw new NotImplementedException();
        }

        private void DataStorageService_TenantCreated(Data.Model.Events.Tenants.TenantCreatedArgs obj)
        {
            _tenantService.CreateTenant(obj);
        }
    }
}
