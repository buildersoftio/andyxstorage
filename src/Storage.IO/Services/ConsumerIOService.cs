using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Model.Events.Messages;
using Buildersoft.Andy.X.Storage.Model.Logs;
using EFCore.BulkExtensions;
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
        private readonly ILogger<ConsumerIOService> _logger;
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly AgentConfiguration _agentConfiguration;

        private ConcurrentQueue<ConsumerLog> consumerLogsQueue;
        private bool IsConsumerLoggingWorking = false;
        private ConcurrentQueue<UnprocessedMessage> unprocessedMessageQueue;
        private bool isUnprocessingMessageProcessorWorking = false;

        private ConcurrentDictionary<string, ConsumerConnector> connectors;


        public ConsumerIOService(
            ILogger<ConsumerIOService> logger,
            PartitionConfiguration partitionConfiguration,
            AgentConfiguration agentConfiguration)
        {
            _logger = logger;
            _partitionConfiguration = partitionConfiguration;
            _agentConfiguration = agentConfiguration;
            consumerLogsQueue = new ConcurrentQueue<ConsumerLog>();

            unprocessedMessageQueue = new ConcurrentQueue<UnprocessedMessage>();

            connectors = new ConcurrentDictionary<string, ConsumerConnector>();

            InitializeAllConsumerConnections();
        }

        private void InitializeAllConsumerConnections()
        {

            var consumers = TenantReader.ReadAllConsumers();
            _logger.LogInformation($"Initializing Consumer Services...");
            foreach (var consumer in consumers)
            {
                InitializeConsumerConnection(consumer.Tenant, consumer.Product, consumer.Component, consumer.Topic, consumer.Name);
            }

            _logger.LogInformation($"Consumer Services are initialized");
        }

        private void InitializeConsumerConnection(string tenant, string product, string component, string topic, string consumer)
        {
            string consumerKey = $"{tenant}-{product}-{component}-{topic}-{consumer}";
            try
            {
                if (connectors.ContainsKey(consumerKey))
                    return;

                var connector = new ConsumerConnector(new TenantContext(ConsumerLocations.GetConsumerPointerFile(tenant,
                    product, component, topic, consumer)), _agentConfiguration.MaxNumber);

                connectors.TryAdd(consumerKey, connector);
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to create consumer connector at '{consumerKey}'");
            }
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
                    _logger.LogInformation($"Consumer '{consumer.Name}' with subscription type '{consumer.SubscriptionType}' at {tenant}/{product}/{component}/{topic} is created");
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
                _logger.LogInformation($"Consumer '{consumer.Name}' with subscription type '{consumer.SubscriptionType}' at {tenant}/{product}/{component}/{topic} is connected");


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
                _logger.LogInformation($"Consumer '{consumer.Name}' with subscription type '{consumer.SubscriptionType}' at {tenant}/{product}/{component}/{topic} is disconnected");

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
            unprocessedMessageQueue.Enqueue(new UnprocessedMessage()
            {
                Tenant = tenant,
                Product = product,
                Component = component,
                Topic = topic,
                MessageId = messageId,
                PartitionFile = partitionFile
            });

            InitializeUnprocessedMessageProcessor();
        }

        #region UnProcessedMessageProcessor
        private void InitializeUnprocessedMessageProcessor()
        {
            if (isUnprocessingMessageProcessorWorking != true)
            {
                isUnprocessingMessageProcessorWorking = true;
                new Thread(() => UnprocessedMessageProcesor()).Start();
            }
        }

        private void UnprocessedMessageProcesor()
        {
            while (unprocessedMessageQueue.IsEmpty != true)
            {
                UnprocessedMessage unprocessedMessage;
                bool isMessageReturned = unprocessedMessageQueue.TryDequeue(out unprocessedMessage);
                if (isMessageReturned == true)
                {
                    var activeConsumerConnectiorKeys = connectors.Keys.Where(x => x.Contains($"{unprocessedMessage.Tenant}-{unprocessedMessage.Product}-{unprocessedMessage.Component}-{unprocessedMessage.Topic}")); //);
                    foreach (var consumerKey in activeConsumerConnectiorKeys)
                    {
                        connectors[consumerKey].MessagesBuffer.Enqueue(new Model.Entities.ConsumerMessage()
                        {
                            MessageId = unprocessedMessage.MessageId,
                            IsAcknowledged = false,
                            SentDate = DateTime.Now,
                            PartitionFile = unprocessedMessage.PartitionFile,
                            PartitionIndex = 0
                        });

                        InitializeMessagingProcessor(consumerKey);
                    }
                }
                else
                    _logger.LogError($"Processing of message failed, couldn't Dequeue un-processed messages");

            }

            isUnprocessingMessageProcessorWorking = false;
        }
        #endregion

        #region MessageProcessor and Update Pointer
        private void InitializeMessagingProcessor(string consumerKey)
        {
            if (connectors[consumerKey].ThreadingPool.AreThreadsRunning != true)
            {
                connectors[consumerKey].ThreadingPool.AreThreadsRunning = true;
                int timeOutCounter = 0;
                while (connectors[consumerKey].TenantContext.Database.CanConnect() != true)
                {
                    timeOutCounter++;
                    Thread.Sleep(500);
                    _logger.LogWarning($"Pointer controller for '{consumerKey}' stopped working, trying to start {timeOutCounter} of 10");
                    if (timeOutCounter == 10)
                    {
                        // recreate connection
                        var consumerKeySplitted = consumerKey.Split('-');

                        connectors.TryRemove(consumerKey, out _);
                        _logger.LogWarning($"Pointer controller for '{consumerKey}' couldn't start. Pointer controller is restarted");

                        connectors[consumerKey].ThreadingPool.AreThreadsRunning = false;
                        return;
                    }
                }
                // initialize all threads here
                connectors[consumerKey].ThreadingPool.AreThreadsRunning = true;
                foreach (var thread in connectors[consumerKey].ThreadingPool.Threads)
                {
                    if (thread.Value.IsThreadWorking != true)
                    {
                        thread.Value.Thread = new Thread(() => MessagingProcessor(consumerKey, thread.Key));
                        thread.Value.Thread.Start();
                        thread.Value.IsThreadWorking = true;
                    }
                }
            }
        }
        private void MessagingProcessor(string consumerKey, Guid threadId)
        {
            while (connectors[consumerKey].MessagesBuffer.IsEmpty != true)
            {
                try
                {
                    Model.Entities.ConsumerMessage message;
                    bool isMessageReturned = connectors[consumerKey].MessagesBuffer.TryDequeue(out message);
                    if (isMessageReturned == true)
                    {
                        BulkAddOrUpdatePointer(consumerKey, message);
                        connectors[consumerKey].Count++;
                    }
                    else
                        _logger.LogError($"Processing of message failed, couldn't Dequeue topic message at {consumerKey}");
                }
                catch (Exception)
                {

                }
            }

            AutoFlushBatchPointers(consumerKey, true);
            connectors[consumerKey].ThreadingPool.Threads[threadId].IsThreadWorking = false;
        }

        private void BulkAddOrUpdatePointer(string consumerKey, Model.Entities.ConsumerMessage message)
        {
            if (connectors[consumerKey].BatchConsumerMessagesToMerge.ContainsKey(message.MessageId))
            {
                if (connectors[consumerKey].BatchConsumerMessagesToMerge[message.MessageId].IsAcknowledged == true)
                    return;

                connectors[consumerKey].BatchConsumerMessagesToMerge[message.MessageId] = message;
            }
            else
                connectors[consumerKey].BatchConsumerMessagesToMerge.TryAdd(message.MessageId, message);

            AutoFlushBatchPointers(consumerKey);
        }

        private void AutoFlushBatchPointers(string consumerKey, bool flushAnyway = false)
        {
            lock (this)
            {
                if (flushAnyway == false)
                {
                    if (connectors[consumerKey].BatchConsumerMessagesToMerge.Count() >= _partitionConfiguration.Size)
                    {
                        connectors[consumerKey].TenantContext.BulkInsertOrUpdate(connectors[consumerKey].BatchConsumerMessagesToMerge.Values.ToList());
                        connectors[consumerKey].BatchConsumerMessagesToMerge.Clear();
                    }
                }
                else
                {
                    if (connectors[consumerKey].BatchConsumerMessagesToMerge.Count() != 0)
                    {
                        connectors[consumerKey].TenantContext.BulkInsertOrUpdate(connectors[consumerKey].BatchConsumerMessagesToMerge.Values.ToList());
                        connectors[consumerKey].BatchConsumerMessagesToMerge.Clear();
                    }
                }
            }
        }
        #endregion

        public string[] TryReadAllLines(string path)
        {
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred during the opening of partition, details={ex.Message}");
                return null;
            }
        }

        public ConsumerConnector GetConsumerConnector(string consumerKey)
        {
            if (connectors.ContainsKey(consumerKey) != true)
            {
                string[] consumerData = consumerKey.Split("-");
                var connector = new ConsumerConnector(new TenantContext(ConsumerLocations.GetConsumerPointerFile(consumerData[0],
    consumerData[1], consumerData[2], consumerData[3], consumerData[4])), _agentConfiguration.MaxNumber);

                connectors.TryAdd(consumerKey, connector);
            }
            return connectors[consumerKey];
        }

    }
}
