using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Storage.Core.Service.System
{
    public class SystemService
    {
        private readonly ILogger<SystemService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SystemService(ILogger<SystemService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            InitializeServices();
        }

        private void InitializeServices()
        {
            var nodes = _serviceProvider.GetService(typeof(List<XNodeConfiguration>));
            var datastorage = _serviceProvider.GetService(typeof(DataStorageConfiguration));
            var agent = _serviceProvider.GetService(typeof(AgentConfiguration));
            var partition = _serviceProvider.GetService(typeof(PartitionConfiguration));

            _logger.LogInformation("Buildersoft");
            _logger.LogInformation("Welcome to Andy X DataStorage");

            _logger.LogInformation("andyx-storage|connecting to andy x node");

            _logger.LogInformation("andyx-storage|starting services");
            _logger.LogInformation("andyx-storage|starting agents");
        }
    }
}
