using Buildersoft.Andy.X.Storage.Data.Model.Events.Tenants;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.IO.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class TenantService : ITenantService
    {
        private readonly ILogger<TenantService> _logger;
        private readonly ITenantRepository _repository;
        public TenantService(ILogger<TenantService> logger, ITenantRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public void CreateTenant(TenantCreatedArgs tenantCreatedArgs)
        {
            Tenant tenant;

            if (_repository.GetAll().ContainsKey(tenantCreatedArgs.TenantName) == true)
                throw new Exception($"There is a tenant registered with this name andyx://{tenantCreatedArgs.TenantName}");

            string tenantLocation = TenantConfigFile.CreateTenantLocation(tenantCreatedArgs.TenantName);

            tenant = new Tenant()
            {
                Id = tenantCreatedArgs.TenantId,
                Name = tenantCreatedArgs.TenantName,
                Encryption = tenantCreatedArgs.Encryption,
                Signature = tenantCreatedArgs.Signature,
                Location = tenantLocation,
                Description = tenantCreatedArgs.TenantDescription,
                Status = tenantCreatedArgs.TenantStatus,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _repository.AddTenant(tenant);

            _logger.LogInformation($"{tenantCreatedArgs.TenantName}: stored");
        }

        public bool DeleteTenant(TenantDeletedArgs tenantDeletedArgs)
        {
            if (_repository.GetAll().ContainsKey(tenantDeletedArgs.TenantName) != true)
                throw new Exception($"There is a tenant registered with this name andyx://{tenantDeletedArgs.TenantName}");

            string tenantLocation = _repository.GetAll()[tenantDeletedArgs.TenantName].Location;
            if (_repository.DeleteTenant(tenantDeletedArgs.TenantName) != true)
                return false;

            TenantConfigFile.RenameStorageDirectory(tenantLocation, "deleted");

            return true;
        }

        public Tenant ReadTenant(TenantReadArgs tenantReadArgs)
        {
            if (_repository.GetAll().ContainsKey(tenantReadArgs.TenantName) != true)
                throw new Exception($"There is a tenant registered with this name andyx://{tenantReadArgs.TenantName}");
            return _repository.GetAll()[tenantReadArgs.TenantName];
        }

        public ConcurrentDictionary<string, Tenant> ReadTenants()
        {
            return _repository.GetAll();
        }

        public Tenant UpdateTenant(TenantUpdatedArgs tenantUpdatedArgs)
        {
            if (_repository.GetAll().ContainsKey(tenantUpdatedArgs.TenantName) != true)
                throw new Exception($"There is a tenant registered with this name andyx://{tenantUpdatedArgs.TenantName}");

            Tenant tenantToUpdate = _repository.GetAll()[tenantUpdatedArgs.TenantName];

            tenantToUpdate.Encryption = tenantToUpdate.Encryption;
            tenantToUpdate.ModifiedDate = DateTime.Now;

            return _repository.UpdateTenant(tenantToUpdate.Name, tenantToUpdate);
        }
    }
}
