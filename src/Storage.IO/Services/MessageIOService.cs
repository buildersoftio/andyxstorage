using Buildersoft.Andy.X.Storage.IO.Connectors;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class MessageIOService
    {
        private readonly ILogger<MessageIOService> _logger;
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly AgentConfiguration _agentConfiguration;
        private readonly ConsumerIOService _consumerIOService;

        private ConcurrentDictionary<string, MessageStorageConnector> connectors;

        public MessageIOService(
            ILogger<MessageIOService> logger,
            PartitionConfiguration partitionConfiguration,
            AgentConfiguration agentConfiguration,
            ConsumerIOService consumerIOService)
        {
            _logger = logger;
            _partitionConfiguration = partitionConfiguration;
            _agentConfiguration = agentConfiguration;
            _consumerIOService = consumerIOService;

            connectors = new ConcurrentDictionary<string, MessageStorageConnector>();
        }

        public bool InitializeMessageFileConnector(string tenant, string product, string component, string topic, DateTime date)
        {
            string topicKey = $"{tenant}~{product}~{component}~{topic}~{date:yyyy_MM_dd_HH}";
            lock (connectors)
            {
                try
                {
                    if (connectors.ContainsKey(topicKey))
                        return true;

                    var connector = new MessageStorageConnector(_partitionConfiguration, _agentConfiguration.MaxNumber);
                    connectors.TryAdd(topicKey, connector);

                    return true;
                }
                catch (Exception)
                {
                    _logger.LogError($"Failed to create message connector at '{topicKey}', retrying");
                    return false;
                }
            }
        }

        public void StoreMessage(Message message)
        {
            string topicKey = AddMessageFileConnectorGetKey(message.Tenant, message.Product, message.Component, message.Topic, message.SentDate);

            connectors[topicKey].MessagesBuffer.Enqueue(new Model.Entities.Message()
            {
                MessageId = message.Id,
                Payload = message.MessageRaw.ToJsonAndEncrypt(),
                Headers = message.Headers.ToJsonAndEncrypt(),
                SentDate = message.SentDate,
                StoredDate = DateTime.Now
            });

            InitializeMessagingProcessor(topicKey, message.SentDate);
        }

        private void InitializeMessagingProcessor(string topicKey, DateTime date)
        {
            if (connectors[topicKey].ThreadingPool.AreThreadsRunning != true)
            {
                connectors[topicKey].ThreadingPool.AreThreadsRunning = true;

                // Wait until is connected to DB.
                int timeOutCounter = 0;
                if (connectors[topicKey].MessageContext == null)
                {
                    var topicData = topicKey.Split("~");
                    connectors[topicKey].MessageContext = new MessageContext(MessageLocations.GetMessagePartitionFile(topicData[0],
                       topicData[1], topicData[2], topicData[3], date));
                    connectors[topicKey].CreateMessageFile();

                    while (connectors[topicKey].MessageContext.Database.CanConnect() != true)
                    {
                        timeOutCounter++;
                        Thread.Sleep(500);
                        _logger.LogWarning($"Message Storage Service for '{topicKey}' stopped working, trying to start {timeOutCounter} of 10");
                        if (timeOutCounter == 10)
                        {
                            connectors[topicKey].ThreadingPool.AreThreadsRunning = false;
                            return;
                        }
                    }
                }

                InitialzeThreads(topicKey);
            }
        }

        private void InitialzeThreads(string topicKey)
        {
            foreach (var thread in connectors[topicKey].ThreadingPool.Threads)
            {
                if (thread.Value.IsThreadWorking != true)
                {
                    try
                    {
                        thread.Value.IsThreadWorking = true;
                        thread.Value.Task = Task.Run(() => MessagingProcessor(topicKey, thread.Key));
                    }
                    catch (Exception)
                    {
                        _logger.LogError($"Message storing thread '{thread.Key}' failed to restart");
                    }
                }
            }
        }

        private void MessagingProcessor(string topicKey, Guid threadId)
        {
            var topicKeySplitted = topicKey.Split('~');
            Model.Entities.Message message;
            while (connectors[topicKey].MessagesBuffer.TryDequeue(out message))
            {
                try
                {
                    connectors[topicKey].BatchMessagesToInsert.TryAdd(message.MessageId, message);
                    _consumerIOService.WriteMessageAsUnackedToAllConsumers(topicKeySplitted[0], topicKeySplitted[1], topicKeySplitted[2], topicKeySplitted[3], message.MessageId, "-1_partition");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error on writing to file details={ex.Message}");
                }
            }
            connectors[topicKey].ThreadingPool.Threads[threadId].IsThreadWorking = false;
        }

        public string AddMessageFileConnectorGetKey(string tenant, string product, string component, string topic, DateTime date)
        {
            string topicKey = $"{tenant}~{product}~{component}~{topic}~{date:yyyy_MM_dd_HH}";

            InitializeMessageFileConnector(tenant, product, component, topic, date);

            return topicKey;
        }

        public MessageContext GetPartitionMessageContext(string topicKey, DateTime date)
        {
            // Wait until is connected to DB.
            int timeOutCounter = 0;
            if (connectors[topicKey].MessageContext == null)
            {
                var topicData = topicKey.Split("~");
                connectors[topicKey].MessageContext = new MessageContext(MessageLocations.GetMessagePartitionFile(topicData[0],
                   topicData[1], topicData[2], topicData[3], date));
                connectors[topicKey].CreateMessageFile();

                while (connectors[topicKey].MessageContext.Database.CanConnect() != true)
                {
                    timeOutCounter++;
                    Thread.Sleep(500);
                    _logger.LogWarning($"Message Storage Service for '{topicKey}' stopped working, trying to start {timeOutCounter} of 10");
                    if (timeOutCounter == 10)
                    {
                        return null;
                    }
                }
            }

            return connectors[topicKey].MessageContext;
        }
    }
}
