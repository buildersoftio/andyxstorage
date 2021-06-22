using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Components;
using Buildersoft.Andy.X.Storage.Model.App.Products;
using Buildersoft.Andy.X.Storage.Model.App.Tenants;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Model.Logs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

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
                new Thread(() => TenantConfigiFileProcessor()).Start();
            }
        }
        private void InitializeTenantLoggingProcessor()
        {
            if (IsTenantLoggingWorking != true)
            {
                IsTenantLoggingWorking = true;
                new Thread(() => TenantLoggingProcessor()).Start();
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
                    logger.LogError("ANDYX-STORAGE#TENANTS|ERROR|Processing of tenant failed, couldn't Dequeue.");
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
                        logger.LogError("ANDYX-STORAGE#TENANTS|ERROR|Logging the incoming requests on tenant failed.");
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

                tenantConfigFilesQueue.Enqueue(tenantDetails);
                InitializeTenantConfigFileProcessor();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
                if (Directory.Exists(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name)) == false)
                {
                    Directory.CreateDirectory(TenantLocations.GetTopicDirectory(tenant, product, component, topic.Name));
                    Directory.CreateDirectory(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic.Name));
                    Directory.CreateDirectory(TenantLocations.GetProducerRootDirectory(tenant, product, component, topic.Name));
                    Directory.CreateDirectory(TenantLocations.GetTopicLogRootDirectory(tenant, product, component, topic.Name));
                    Directory.CreateDirectory(TenantLocations.GetIndexRootDirectory(tenant, product, component, topic.Name));
                    Directory.CreateDirectory(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic.Name));

                    if (File.Exists(TenantLocations.GetMessagePartitionFile(tenant, product, component, topic.Name, topic.ActiveMessagePartitionFile)) == false)
                        File.Create(TenantLocations.GetMessagePartitionFile(tenant, product, component, topic.Name, topic.ActiveMessagePartitionFile)).Close();

                    // Because this call is triggered by XNode in only in an agent in storage, it doesn't need to go thru a queue.
                    TenantWriter.WriteTopicConfigFile(tenant, product, component, topic);
                }
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
            string log = $"{DateTime.Now:HH:mm:ss}|ANDYX-STORAGE#AGENTS|{agentId}|{state}";
            tenantLogsQueue.Enqueue(new TenantLog() { Tenant = tenantName, Log = log });

            InitializeTenantLoggingProcessor();
        }
    }
}
