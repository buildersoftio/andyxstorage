using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.Logs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class ConsumerIOService
    {
        private readonly ILogger<ConsumerIOService> logger;
        private Queue<ConsumerLog> consumerLogsQueue;
        private bool IsConsumerLoggingWorking = false;

        public ConsumerIOService(ILogger<ConsumerIOService> logger)
        {
            this.logger = logger;
            consumerLogsQueue = new Queue<ConsumerLog>();
        }

        private void InitializeConsumerLoggingProcessor()
        {
            if (IsConsumerLoggingWorking != true)
            {
                IsConsumerLoggingWorking = true;
                new Thread(() => ConsumerLoggingProcessor()).Start();
            }
        }

        private void ConsumerLoggingProcessor()
        {
            while (consumerLogsQueue.Count > 0)
            {
                var consumerLog = consumerLogsQueue.Dequeue();
                ConsumerWriter.WriteInConsumerLogFile(consumerLog.Tenant, consumerLog.Product, consumerLog.Component, consumerLog.Topic, consumerLog.ConsumerName, consumerLog.Log);
            }
            IsConsumerLoggingWorking = false;
        }

        public bool TryCreateConsumerDirectory(string tenant, string product, string component, string topic, Consumer consumer)
        {
            try
            {
                if (Directory.Exists(ConsumerLocations.GetConsumerDirectory(tenant, product, component, topic, consumer.Name)) != true)
                {
                    Directory.CreateDirectory(ConsumerLocations.GetConsumerDirectory(tenant, product, component, topic, consumer.Name));
                    Directory.CreateDirectory(ConsumerLocations.GetConsumerLogsDirectory(tenant, product, component, topic, consumer.Name));
                    consumerLogsQueue.Enqueue(new ConsumerLog()
                    {
                        Tenant = tenant,
                        Product = product,
                        Component = component,
                        Topic = topic,
                        ConsumerName = consumer.Name,
                        Log = $"{DateTime.Now:HH:mm:ss}|CONSUMER#|{consumer.Name}|{consumer.SubscriptionType}|{consumer.Id}|CREATED"
                    });

                    ConsumerWriter.WriteConsumerConfigFile(tenant, product, component, topic, consumer);
                    logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|{tenant}|{product}|{component}|{topic}|{consumer.Name}|{consumer.SubscriptionType}|{consumer.Id}|CREATED");

                }

                consumerLogsQueue.Enqueue(new ConsumerLog()
                {
                    Tenant = tenant,
                    Product = product,
                    Component = component,
                    Topic = topic,
                    ConsumerName = consumer.Name,
                    Log = $"{DateTime.Now:HH:mm:ss}|CONSUMER#|{consumer.Name}|{consumer.SubscriptionType}|{consumer.Id}|CONNECTED"
                });

                InitializeConsumerLoggingProcessor();
                logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|{tenant}|{product}|{component}|{topic}|{consumer.Name}|{consumer.SubscriptionType}|{consumer.Id}|CONNECTED");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool WriteDisconnectedConsumerLog(string tenant, string product, string component, string topic, Consumer consumer)
        {
            try
            {
                logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|{tenant}|{product}|{component}|{topic}|{consumer.Name}|{consumer.Id}|DISCONNECTED");
                consumerLogsQueue.Enqueue(new ConsumerLog()
                {
                    Tenant = tenant,
                    Product = product,
                    Component = component,
                    Topic = topic,
                    ConsumerName = consumer.Name,
                    Log = $"{DateTime.Now:HH:mm:ss}|CONSUMER#|{consumer.Name}|{consumer.Id}|DISCONNECTED"
                });
                InitializeConsumerLoggingProcessor();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
