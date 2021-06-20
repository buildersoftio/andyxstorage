using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Producers;
using Buildersoft.Andy.X.Storage.Model.Logs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class ProducerIOService
    {
        private readonly ILogger<ProducerIOService> logger;
        private Queue<ProducerLog> producerLogsQueue;
        private bool IsProducerLoggingWorking = false;

        public ProducerIOService(ILogger<ProducerIOService> logger)
        {
            this.logger = logger;
            producerLogsQueue = new Queue<ProducerLog>();
        }

        private void InitializeProducerLoggingProcessor()
        {
            if (IsProducerLoggingWorking != true)
            {
                IsProducerLoggingWorking = true;
                new Thread(() => ProducerLoggingProcessor()).Start();
            }
        }

        private void ProducerLoggingProcessor()
        {
            while (producerLogsQueue.Count > 0)
            {
                var producerLog = producerLogsQueue.Dequeue();
                ProducerWriter.WriteInProducerLogFile(producerLog.Tenant, producerLog.Product, producerLog.Component, producerLog.Topic, producerLog.ProducerName, producerLog.Log);
            }
            IsProducerLoggingWorking = false;
        }

        public bool TryCreateProducerDirectory(string tenant, string product, string component, string topic, Producer producer)
        {
            try
            {
                if (Directory.Exists(ProducerLocations.GetProducerDirectory(tenant, product, component, topic, producer.Name)) != true)
                {
                    Directory.CreateDirectory(ProducerLocations.GetProducerDirectory(tenant, product, component, topic, producer.Name));
                    Directory.CreateDirectory(ProducerLocations.GetProducerLogsDirectory(tenant, product, component, topic, producer.Name));
                    producerLogsQueue.Enqueue(new ProducerLog()
                    {
                        Tenant = tenant,
                        Product = product,
                        Component = component,
                        Topic = topic,
                        ProducerName = producer.Name,
                        Log = $"{DateTime.Now:HH:mm:ss}|PRODUCER#|{producer.Name}|{producer.Id}|CREATED"
                    });
                    ProducerWriter.WriteProducerConfigFile(tenant, product, component, topic, producer);
                    logger.LogInformation($"ANDYX-STORAGE#PRODUCERS|{tenant}|{product}|{component}|{topic}|{producer.Name}|{producer.Id}|CREATED");
                }

                // Write log file
                producerLogsQueue.Enqueue(new ProducerLog()
                {
                    Tenant = tenant,
                    Product = product,
                    Component = component,
                    Topic = topic,
                    ProducerName = producer.Name,
                    Log = $"{DateTime.Now:HH:mm:ss}|PRODUCER#|{producer.Name}|{producer.Id}|CONNECTED"
                });

                InitializeProducerLoggingProcessor();
                logger.LogInformation($"ANDYX-STORAGE#PRODUCERS|{tenant}|{product}|{component}|{topic}|{producer.Name}|{producer.Id}|CONNECTED");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WriteDisconnectedProducerLog(string tenant, string product, string component, string topic, Producer producer)
        {
            try
            {
                logger.LogInformation($"ANDYX-STORAGE#PRODUCERS|{tenant}|{product}|{component}|{topic}|{producer.Name}|{producer.Id}|DISCONNECTED");
                producerLogsQueue.Enqueue(new ProducerLog()
                {
                    Tenant = tenant,
                    Product = product,
                    Component = component,
                    Topic = topic,
                    ProducerName = producer.Name,
                    Log = $"{DateTime.Now:HH:mm:ss}|PRODUCER#|{producer.Name}|{producer.Id}|DISCONNECTED"
                });
                InitializeProducerLoggingProcessor();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
