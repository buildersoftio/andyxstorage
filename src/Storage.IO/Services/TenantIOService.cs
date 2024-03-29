﻿using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Components;
using Buildersoft.Andy.X.Storage.Model.App.Products;
using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Model.Logs;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class TenantIOService
    {
        private readonly ILogger<TenantIOService> logger;

        private ConcurrentQueue<Tenant> tenantConfigFilesQueue;
        private ConcurrentQueue<TenantLog> tenantLogsQueue;
        private bool IsTenantConfigFilesWorking = false;
        private bool IsTenantLoggingWorking = false;

        public TenantIOService(ILogger<TenantIOService> logger)
        {
            this.logger = logger;

            tenantConfigFilesQueue = new ConcurrentQueue<Tenant>();
            tenantLogsQueue = new ConcurrentQueue<TenantLog>();
        }

        private void InitializeTenantConfigFileProcessor()
        {
            if (IsTenantConfigFilesWorking != true)
            {
                IsTenantConfigFilesWorking = true;
                new Task(() => TenantConfigiFileProcessor()).Start();
            }
        }
        private void InitializeTenantLoggingProcessor()
        {
            if (IsTenantLoggingWorking != true)
            {
                IsTenantLoggingWorking = true;
                new Task(() => TenantLoggingProcessor()).Start();
            }
        }

        private void TenantConfigiFileProcessor()
        {
            while (tenantConfigFilesQueue.Count > 0)
            {
                Tenant tenant;
                bool isTenantReturned = tenantConfigFilesQueue.TryDequeue(out tenant);
                if (isTenantReturned == true)
                    TenantWriter.WriteTenantConfigFile(tenant);
                else
                    logger.LogError("Processing of tenant failed, couldn't Dequeue");
            }
            IsTenantConfigFilesWorking = false;
        }

        private void TenantLoggingProcessor()
        {
            while (tenantLogsQueue.Count > 0)
            {
                try
                {
                    TenantLog tenantLog;
                    bool isLogReturned = tenantLogsQueue.TryDequeue(out tenantLog);
                    if (isLogReturned == true)
                        TenantWriter.WriteInTenantLog(tenantLog.Tenant, tenantLog.Log);
                    else
                        logger.LogError("Logging requests on tenant failed");
                }
                catch (Exception)
                {
                    // Console.WriteLine($"ERROR on queue: {tenantLogsQueue.Count}");
                }

            }
            IsTenantLoggingWorking = false;
        }

        public bool TryCreateTenantDirectory(string tenantName, Tenant tenantDetails)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetTenantDirectory(tenantName)) == true)
                {
                    // Update the file from the Server
                    tenantConfigFilesQueue.Enqueue(tenantDetails);
                    InitializeTenantConfigFileProcessor();
                    return true;
                }

                Directory.CreateDirectory(TenantLocations.GetTenantDirectory(tenantName));
                Directory.CreateDirectory(TenantLocations.GetProductRootDirectory(tenantName));
                Directory.CreateDirectory(TenantLocations.GetTenantLogsRootDirectory(tenantName));
                Directory.CreateDirectory(TenantLocations.GetTenantTokensDirectory(tenantName));

                tenantConfigFilesQueue.Enqueue(tenantDetails);
                InitializeTenantConfigFileProcessor();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryUpdateTenantDirectory(string tenantName, Tenant tenantDetails)
        {
            if (Directory.Exists(TenantLocations.GetTenantDirectory(tenantName)) == true)
            {
                // Update the file from the Server
                tenantConfigFilesQueue.Enqueue(tenantDetails);
                InitializeTenantConfigFileProcessor();
            }
            return true;
        }

        public bool TryCreateProductDirectory(string tenant, Product product)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetProductDirectory(tenant, product.Name)) == false)
                {
                    Directory.CreateDirectory(TenantLocations.GetProductDirectory(tenant, product.Name));
                    Directory.CreateDirectory(TenantLocations.GetComponentRootDirectory(tenant, product.Name));

                    // Because this call is triggered by XNode in only in an agent in storage, it doesn't need to go thru a queue.
                    TenantWriter.WriteProductConfigFile(tenant, product);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryCreateComponentDirectory(string tenant, string product, Component component)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetComponentDirectory(tenant, product, component.Name)) == false)
                {
                    Directory.CreateDirectory(TenantLocations.GetComponentDirectory(tenant, product, component.Name));
                    Directory.CreateDirectory(TenantLocations.GetTopicRootDirectory(tenant, product, component.Name));
                    Directory.CreateDirectory(TenantLocations.GetComponentTokenDirectory(tenant, product, component.Name));

                    // Because this call is triggered by XNode in only in an agent in storage, it doesn't need to go thru a queue.
                    TenantWriter.WriteComponentConfigFile(tenant, product, component);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryCreateTopicDirectory(string tenant, string product, string component, Topic topic)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetProducerRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetProducerRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetTopicLogRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetTopicLogRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetIndexRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetIndexRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic.Name));

                // Because this call is triggered by XNode in only in an agent in storage, it doesn't need to go thru a queue.
                TenantWriter.WriteTopicConfigFile(tenant, product, component, topic);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool TryUpdateTopicDirectory(string tenant, string product, string component, Topic topic)
        {
            try
            {
                if (Directory.Exists(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetProducerRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetProducerRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetTopicLogRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetTopicLogRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetIndexRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetIndexRootDirectory(tenant, product, component, topic.Name));
                if (Directory.Exists(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic.Name)) != true)
                    Directory.CreateDirectory(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic.Name));

                if (File.Exists(TenantLocations.GetTopicConfigFile(tenant, product, component, topic.Name)) != true)
                {
                    // Because this call is triggered by XNode in only in an agent in storage, it doesn't need to go thru a queue.
                    TenantWriter.WriteTopicConfigFile(tenant, product, component, topic);
                    return true;
                }

                // Do not update the topic schema
                //Topic fromFile = TenantReader.ReadTopicConfigFile(tenant, product, component, topic.Name);
                //fromFile.Schema = topic.Schema;
                //TenantWriter.WriteTopicConfigFile(tenant, product, component, fromFile);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void WriteAgentStateInTenantLog(string tenantName, string agentId, string state)
        {
            string log = $"{DateTime.Now}\tandyx-storage\tagent with id '{agentId}' is '{state}'";
            tenantLogsQueue.Enqueue(new TenantLog() { Tenant = tenantName, Log = log });

            InitializeTenantLoggingProcessor();
        }


        public bool CreateTenantTokenFile(string tenant, TenantToken tenantToken)
        {
            string fileName = $"tkn_{Guid.NewGuid()}.xandy";
            return TenantWriter.WriteTenantTokenFile(fileName, tenant, tenantToken);
        }

        public bool DeleteTenantTokenFile(string tenant, string token)
        {
            var tokenFiles = Directory.GetFiles(TenantLocations.GetTenantTokensDirectory(tenant));
            foreach (var tokenFile in tokenFiles)
            {
                var tokenDetails = File.ReadAllText(tokenFile).JsonToObject<TenantToken>();
                if (tokenDetails != null)
                {
                    if (tokenDetails.Token == token)
                    {
                        File.Delete(tokenFile);
                        break;
                    }
                }
            }
            return true;
        }

        public bool CreateComponentTokenFile(string tenant, string product, string component, ComponentToken componentToken)
        {
            string fileName = $"tkn_{Guid.NewGuid()}.xandy";
            return TenantWriter.WriteComponentTokenFile(tenant, product, component, fileName, componentToken);
        }

        public bool DeleteComponentTokenFile(string tenant, string product, string component,string token)
        {
            var tokenFiles = Directory.GetFiles(TenantLocations.GetComponentTokenDirectory(tenant, product, component));
            foreach (var tokenFile in tokenFiles)
            {
                var tokenDetails = File.ReadAllText(tokenFile).JsonToObject<ComponentToken>();
                if (tokenDetails != null)
                {
                    if (tokenDetails.Token == token)
                    {
                        File.Delete(tokenFile);
                        break;
                    }
                }
            }
            return true;
        }
    }
}
