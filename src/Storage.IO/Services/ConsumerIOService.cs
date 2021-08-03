﻿using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.Model.Logs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class ConsumerIOService
    {
        private readonly ILogger<ConsumerIOService> logger;
        private readonly PartitionConfiguration partitionConfiguration;

        private ConcurrentQueue<ConsumerLog> consumerLogsQueue;
        private bool IsConsumerLoggingWorking = false;

        private ConcurrentDictionary<string, ConsumerConnector> connectors;


        public ConsumerIOService(ILogger<ConsumerIOService> logger, PartitionConfiguration partitionConfiguration)
        {
            this.logger = logger;
            this.partitionConfiguration = partitionConfiguration;
            consumerLogsQueue = new ConcurrentQueue<ConsumerLog>();
            connectors = new ConcurrentDictionary<string, ConsumerConnector>();

            InitializeAllConsumerConnections();
        }

        private void InitializeAllConsumerConnections()
        {

            var consumers = TenantReader.ReadAllConsumers();
            logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|INITIALIZING");
            foreach (var consumer in consumers)
            {
                InitializeConsumerConnection(consumer.Tenant, consumer.Product, consumer.Component, consumer.Topic, consumer.Name);
            }

            logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|INITIALIZED");
        }

        private void InitializeConsumerConnection(string tenant, string product, string component, string topic, string consumer)
        {
            string consumerKey = $"{tenant}-{product}-{component}-{topic}-{consumer}";
            if (connectors.ContainsKey(consumerKey))
                return;

            var connector = new ConsumerConnector(new TenantContext(ConsumerLocations.GetConsumerPointerFile(tenant,
                product, component, topic, consumer)))
            {
                IsProcessorWorking = false
            };

            connectors.TryAdd(consumerKey, connector);
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
                ConsumerLog consumerLog;
                var isDequeued = consumerLogsQueue.TryDequeue(out consumerLog);
                if (isDequeued == true)
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
                    logger.LogInformation($"ANDYX-STORAGE#CONSUMERS|{tenant}|{product}|{component}|{topic}|{consumer.Name}|{consumer.SubscriptionType}|{consumer.Id}|CREATED");


                }

                ConsumerWriter.WriteConsumerConfigFile(tenant, product, component, topic, consumer);
                InitializeConsumerConnection(tenant, product, component, topic, consumer.Name);

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

        private string AddConsumerConnectorGetKey(string tenant, string product, string component, string topic, string consumer)
        {
            string consumerKey = $"{tenant}-{product}-{component}-{topic}-{consumer}";

            InitializeConsumerConnection(tenant, product, component, topic, consumer);

            return consumerKey;
        }

        // Acknowledgement of messages
        public void WriteMessageAcknowledged(MessageAcknowledgedArgs message, string partitionFile = "none")
        {
            string consumerKey = AddConsumerConnectorGetKey(message.Tenant, message.Product, message.Component, message.Topic, message.Consumer);

            if (message.IsAcknowledged == true)
                connectors[consumerKey].MessagesBuffer.Enqueue(new Model.Entities.ConsumerMessage()
                {
                    MessageId = message.MessageId,
                    IsAcknowledged = message.IsAcknowledged,
                    AcknowledgedDate = DateTime.Now,
                    SentDate = DateTime.Now,
                    PartitionFile = partitionFile,
                    PartitionIndex = 0
                });
            else
                connectors[consumerKey].MessagesBuffer.Enqueue(new Model.Entities.ConsumerMessage()
                {
                    MessageId = message.MessageId,
                    IsAcknowledged = message.IsAcknowledged,
                    SentDate = DateTime.Now,
                    PartitionFile = partitionFile,
                    PartitionIndex = 0
                });

            InitializeMessagingProcessor(consumerKey);
        }

        public void WriteMessageAsUnackedToAllConsumers(string tenant, string product, string component, string topic, Guid messageId, string partitionFile)
        {
            // string consumerKey = $"{tenant}-{product}-{component}-{topic}-{consumer}";
            var activeConsumerConnectiorKeys = connectors.Keys.Where(x => x.Contains($"{tenant}-{product}-{component}-{topic}")); //);
            foreach (var consumerKey in activeConsumerConnectiorKeys)
            {
                connectors[consumerKey].MessagesBuffer.Enqueue(new Model.Entities.ConsumerMessage()
                {
                    MessageId = messageId,
                    IsAcknowledged = false,
                    SentDate = DateTime.Now,
                    PartitionFile = partitionFile,
                    PartitionIndex = 0
                });

                InitializeMessagingProcessor(consumerKey);
            }
        }

        private void InitializeMessagingProcessor(string consumerKey)
        {
            if (connectors[consumerKey].IsProcessorWorking != true)
            {
                connectors[consumerKey].IsProcessorWorking = true;
                new Thread(() => MessagingProcessor(consumerKey)).Start();
            }
        }

        private void MessagingProcessor(string consumerKey)
        {
            while (connectors[consumerKey].MessagesBuffer.Count > 0)
            {
                try
                {
                    Model.Entities.ConsumerMessage message;
                    bool isMessageReturned = connectors[consumerKey].MessagesBuffer.TryDequeue(out message);
                    if (isMessageReturned == true)
                    {
                        UpdatePointer(consumerKey, message);
                        connectors[consumerKey].Count++;
                    }
                    else
                        logger.LogError($"ANDYX-STORAGE#MESSAGES|ERROR|Processing of message failed, couldn't Dequeue.|TOPIC|{consumerKey}");
                    // Increase the Counter
                }
                catch (Exception)
                {

                }
            }

            //connectors[consumerKey].TenantContext.SaveChanges();
            connectors[consumerKey].IsProcessorWorking = false;
        }

        private void UpdatePointer(string consumerKey, Model.Entities.ConsumerMessage message)
        {
            var messageIndb = connectors[consumerKey].TenantContext.ConsumerMessages.Find(message.MessageId);
            if (messageIndb != null)
            {
                // update
                messageIndb.IsAcknowledged = message.IsAcknowledged;
                messageIndb.AcknowledgedDate = message.AcknowledgedDate;
                connectors[consumerKey].TenantContext.ConsumerMessages.Update(messageIndb);
            }
            else
            {
                // add
                connectors[consumerKey].TenantContext.ConsumerMessages.Add(message);
            }

            connectors[consumerKey].TenantContext.SaveChanges();

            // TODO: new solution
            // Check this one, I think it should be less than partition size
            //if (connectors[consumerKey].Count >= partitionConfiguration.Size)
            //{
            //    // flush data into disk
            //    connectors[consumerKey].Count = 0;
            //    connectors[consumerKey].TenantContext.SaveChanges();
            //}
        }

        public string[] TryReadAllLines(string path)
        {
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error occurred during the opening of partition, details={ex.Message}");
                return null;
            }
        }

    }
}
