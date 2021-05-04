using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.Core.Service.System
{
    public class SystemService
    {
        private readonly ILogger<SystemService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly List<XNodeConfiguration> nodes;
        private readonly DataStorageConfiguration dataStorage;
        private readonly AgentConfiguration agent;
        private readonly PartitionConfiguration partition;
        private readonly CredentialsConfiguration credentials;

        public SystemService(ILogger<SystemService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            nodes = _serviceProvider.GetService(typeof(List<XNodeConfiguration>)) as List<XNodeConfiguration>;
            dataStorage = _serviceProvider.GetService(typeof(DataStorageConfiguration)) as DataStorageConfiguration;
            agent = _serviceProvider.GetService(typeof(AgentConfiguration)) as AgentConfiguration;
            partition = _serviceProvider.GetService(typeof(PartitionConfiguration)) as PartitionConfiguration;
            credentials = _serviceProvider.GetService(typeof(CredentialsConfiguration)) as CredentialsConfiguration;

            DoFileConfiguration();
            InitializeServices();
        }

        private void DoFileConfiguration()
        {
            if (Directory.Exists(SystemLocations.GetDataDirectory()))
                return;

            _logger.LogInformation("Configuring Andy X DataStorage");
            _logger.LogWarning($"If this process failes make sure that this user have access to write in this location {SystemLocations.GetRootDirectory()}");

            SystemIOService.CreateConfigDirectories();
        }

        private void InitializeServices()
        {

            _logger.LogInformation("Buildersoft");
            _logger.LogInformation("Welcome to Andy X DataStorage");

            _logger.LogInformation("andyx-storage - connecting to andy x node");

            _logger.LogInformation("andyx-storage - starting services");
            _logger.LogInformation("andyx-storage - starting agents");
        }
    }
}
