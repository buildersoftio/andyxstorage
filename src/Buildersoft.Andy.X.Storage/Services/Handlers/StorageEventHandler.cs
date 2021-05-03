using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.IO.Configurations;
using Buildersoft.Andy.X.Storage.IO.Storage.Tenants;
using Microsoft.Extensions.Logging;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class StorageEventHandler
    {
        private readonly ILogger<StorageEventHandler> _logger;
        private readonly NodeDataStorageService _service;
        private readonly DataStorage _dataStorage;
        public StorageEventHandler(ILogger<StorageEventHandler> logger, NodeDataStorageService service)
        {
            _logger = logger;
            _service = service;
            _dataStorage = ConfigFile.GetDataStorageSettings();
            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.DataStorageConnected += DataStorageService_DataStorageConnected;
            _service.DataStorageDisconnected += DataStorageService_DataStorageDisconnected;
        }

        private void DataStorageService_DataStorageDisconnected(Data.Model.Events.DataStorageDisconnectedArgs obj)
        {
            _logger.LogWarning($"andyx-storage://{_dataStorage.DataStorageName}?terminalId={obj.Id}: disconnected");
        }

        private async void DataStorageService_DataStorageConnected(Data.Model.Events.DataStorageConnectedArgs obj)
        {
            _logger.LogInformation($"andyx-storage://{_dataStorage.DataStorageName}?terminalId={obj.Id}: connected");

            // Sendback to Andy X, Tenants with their own properties
            await _service.SendStorageCurrentState(TenantConfigFile.GetTenants());
        }
    }
}
