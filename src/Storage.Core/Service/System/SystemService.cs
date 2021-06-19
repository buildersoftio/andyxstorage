using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Service.XNodes;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.IO.Writers;
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
        private readonly IXNodeConnectionRepository _xNodeConnectionRepository;
        private readonly SystemIOService _systemIOService;
        private readonly TenantIOService _tenantIOService;
        private readonly List<XNodeConfiguration> nodes;
        private readonly DataStorageConfiguration dataStorage;
        private readonly AgentConfiguration agent;
        private readonly PartitionConfiguration partition;
        private readonly CredentialsConfiguration credentials;


        public SystemService(ILogger<SystemService> logger, IServiceProvider serviceProvider, IXNodeConnectionRepository xNodeConnectionRepository, SystemIOService systemIOService, TenantIOService tenantIOService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _xNodeConnectionRepository = xNodeConnectionRepository;
            _systemIOService = systemIOService;
            _tenantIOService = tenantIOService;
            nodes = _serviceProvider.GetService(typeof(List<XNodeConfiguration>)) as List<XNodeConfiguration>;
            dataStorage = _serviceProvider.GetService(typeof(DataStorageConfiguration)) as DataStorageConfiguration;
            agent = _serviceProvider.GetService(typeof(AgentConfiguration)) as AgentConfiguration;
            partition = _serviceProvider.GetService(typeof(PartitionConfiguration)) as PartitionConfiguration;
            credentials = _serviceProvider.GetService(typeof(CredentialsConfiguration)) as CredentialsConfiguration;


            DoFileConfiguration();

            UpdateXNodesConfiguration();
            UpdateDataStorageConfiguration();
            UpdateCredentials();

            InitializeServices();
        }

        private void UpdateXNodesConfiguration()
        {
            List<XNodeConfiguration> xNodes = nodes;
            if (File.Exists(SystemLocations.GetNodesConfigFile()))
            {
                xNodes = SystemConfigurationReader.ReadXNodesConfigurationFromFile();
                foreach (var node in nodes)
                {
                    var nodeExists = xNodes.Exists(x => x.ServiceUrl == node.ServiceUrl);
                    if (nodeExists != true)
                        xNodes.Add(node);
                }
            }
            SystemConfigurationWriter.WriteXNodesConfigurationFromFile(xNodes);
        }

        private void UpdateDataStorageConfiguration()
        {
            DataStorageConfiguration newConfig = dataStorage;
            if (File.Exists(SystemLocations.GetStorageCredentialsConfigFile()))
            {
                var actualConfig = SystemConfigurationReader.ReadStorageConfigurationFromFile();
                if (newConfig.Name != actualConfig.Name || newConfig.Status != actualConfig.Status)
                    SystemConfigurationWriter.WriteStorageConfigurationFromFile(newConfig);
            }
            else
                SystemConfigurationWriter.WriteStorageConfigurationFromFile(newConfig);
        }

        private void UpdateCredentials()
        {
            CredentialsConfiguration newConfig = credentials;
            if (File.Exists(SystemLocations.GetCredentialsConfigFile()))
            {
                var actualConfig = SystemConfigurationReader.ReadCredentialsConfigurationFromFile();
                if (newConfig.Username != actualConfig.Username || newConfig.Password != actualConfig.Password)
                    SystemConfigurationWriter.WriteCredentialsConfigurationFromFile(newConfig);
            }
            else
                SystemConfigurationWriter.WriteCredentialsConfigurationFromFile(newConfig);
        }

        private void DoFileConfiguration()
        {
            if (Directory.Exists(SystemLocations.GetDataDirectory()))
                return;
            _logger.LogInformation("ANDYX-STORAGE#CONFIGURING");
            _logger.LogInformation($"ANDYX-STORAGE#CONFIGURING|If this process failes make sure that this user have access to write in this location {SystemLocations.GetRootDirectory()}");

            _systemIOService.CreateConfigDirectories();
        }

        private void InitializeServices()
        {
            _logger.LogInformation("Buildersoft");
            _logger.LogInformation("Buildersoft Andy X Storage");

            _logger.LogInformation("ANDYX-STORAGE#SERVICES|STARTING");
            _logger.LogInformation("ANDYX-STORAGE#AGENTS|STARTING");

            foreach (var xnode in nodes)
            {
                if (agent.LoadBalanced == false)
                {
                    for (int i = 0; i < agent.MaxNumber; i++)
                    {
                        string agentId = Guid.NewGuid().ToString();
                        _logger.LogInformation($"ANDYX-STORAGE#AGENT|{agentId}|CONNECTING");
                        var nodeEventsService = new XNodeEventService(_logger, agentId, xnode, dataStorage, agent, _xNodeConnectionRepository, _tenantIOService);
                    }
                }
                else
                {
                    _logger.LogError($"ANDYX-STORAGE#AGENT|STARTING|LoadBalanced EQ true|UNSUPPORTED");
                }
            }
        }
    }
}
