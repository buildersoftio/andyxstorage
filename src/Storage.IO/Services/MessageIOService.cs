using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
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

        private ConcurrentDictionary<string, MessageFileGate> topicsActiveFiles;

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

            topicsActiveFiles = new ConcurrentDictionary<string, MessageFileGate>();
        }

        private void InitializeMessagingProcessor(string topicKey)
        {
            if (topicsActiveFiles[topicKey].ThreadPool.AreThreadsRunning != true)
            {
                topicsActiveFiles[topicKey].ThreadPool.AreThreadsRunning = true;
                foreach (var thread in topicsActiveFiles[topicKey].ThreadPool.Threads)
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
        }

        private void MessagingProcessor(string topicKey, Guid threadId)
        {
            while (topicsActiveFiles[topicKey].MessagesBuffer.Count > 0)
            {
                try
                {
                    Message message;
                    bool isMessageReturned = topicsActiveFiles[topicKey].MessagesBuffer.TryDequeue(out message);
                    if (isMessageReturned == true)
                    {
                        ProcessMessageToFile(topicKey, message);
                        topicsActiveFiles[topicKey].RowsCount++;
                    }
                    else
                        _logger.LogError($"Processing of message failed, couldn't Dequeue topic message at {topicKey}");
                }
                catch (Exception)
                {

                }
            }
            // Flush all messages to disk
            AutoFlush(topicKey);
            topicsActiveFiles[topicKey].ThreadPool.Threads[threadId].IsThreadWorking = false;
        }

        private void ProcessMessageToFile(string topicKey, Message message)
        {
            if (topicsActiveFiles[topicKey].MessageDetailsFileStream == null)
            {
                // Will create a new FileStream
                Topic topic = TenantReader.ReadTopicConfigFile(message.Tenant, message.Product, message.Component, message.Topic);
                string newFileLocation = TenantLocations.GetMessagePartitionFile(message.Tenant, message.Product, message.Component, message.Topic, topic.ActiveMessagePartitionFile);
                int countRowsInPartition = File.ReadAllLines(newFileLocation).Length;

                // Initialize message detail file
                topicsActiveFiles[topicKey].MessageDetailsFileStream =
                    new FileStream(newFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                topicsActiveFiles[topicKey].RowsCount = countRowsInPartition;

                topicsActiveFiles[topicKey].MessageDetailsStreamWriter = new StreamWriter(topicsActiveFiles[topicKey].MessageDetailsFileStream);
                topicsActiveFiles[topicKey].ActivePartitionFile = topic.ActiveMessagePartitionFile;
                // Initialize IdKey Index File
                string newIdFileLocation = TenantLocations.GetIdKeyIndexFile(message.Tenant, message.Product, message.Component, message.Topic, $"primary_key_{topic.ActiveMessagePartitionFile}");
                topicsActiveFiles[topicKey].IdKeyFileStream = new FileStream(newIdFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                topicsActiveFiles[topicKey].IdKeyStreamWriter = new StreamWriter(topicsActiveFiles[topicKey].IdKeyFileStream);
            }

            // Check if the file has partitionSized lines
            CheckAndChangePartition(message, topicsActiveFiles[topicKey]);

            // Write message
            topicsActiveFiles[topicKey].MessageDetailsStreamWriter.WriteLineAsync(new MessageRow() { Id = message.Id, MessageRaw = message.MessageRaw }.ToJsonAndEncrypt());
            topicsActiveFiles[topicKey].IdKeyStreamWriter.WriteLineAsync(message.Id.ToString());

            // Write as unacked message to consumers
            _consumerIOService.WriteMessageAsUnackedToAllConsumers(message.Tenant, message.Product, message.Component, message.Topic, message.Id, topicsActiveFiles[topicKey].ActivePartitionFile);
        }

        public void WriteMessageInFile(Message message)
        {
            string topicKey = $"{message.Tenant}-{message.Product}-{message.Component}-{message.Topic}";
            if (topicsActiveFiles.ContainsKey(topicKey) != true)
            {
                var fileGate = new MessageFileGate(_agentConfiguration.MaxNumber)
                {
                    MessageDetailsFileStream = null,
                    RowsCount = -1
                };

                topicsActiveFiles.TryAdd(topicKey, fileGate);
            }

            topicsActiveFiles[topicKey].MessagesBuffer.Enqueue(message);
            InitializeMessagingProcessor(topicKey);
        }

        private void CheckAndChangePartition(Message message, MessageFileGate fileGate)
        {

            if (fileGate.RowsCount >= _partitionConfiguration.Size)
            {
                // Close connection with current partition
                AutoFlush(fileGate);

                fileGate.MessageDetailsStreamWriter.Close();
                fileGate.MessageDetailsFileStream.Close();
                fileGate.IdKeyStreamWriter.Close();
                fileGate.IdKeyFileStream.Close();

                Topic topic = TenantReader.ReadTopicConfigFile(message.Tenant, message.Product, message.Component, message.Topic);
                if (DateTime.Now.ToString("dd-MM-yyyy") != topic.PartitionDate)
                    topic.PartitionIndex = 0;

                topic.PartitionIndex++;
                topic.PartitionDate = DateTime.Now.ToString("dd-MM-yyyy");
                topic.MessagePartitionFiles.Add(topic.ActiveMessagePartitionFile);
                topic.ActiveMessagePartitionFile = $"msg_{DateTime.Now:MM_yyyy}_partition_{topic.PartitionIndex}.xand";
                TenantWriter.WriteTopicConfigFile(message.Tenant, message.Product, message.Component, topic);

                string newFileLocation = TenantLocations.GetMessagePartitionFile(message.Tenant, message.Product, message.Component, message.Topic, topic.ActiveMessagePartitionFile);
                // Create File
                if (File.Exists(newFileLocation) == false)
                    File.Create(newFileLocation).Close();

                fileGate.RowsCount = 0;
                fileGate.MessageDetailsFileStream = new FileStream(newFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                fileGate.MessageDetailsStreamWriter = new StreamWriter(fileGate.MessageDetailsFileStream);
                fileGate.ActivePartitionFile = topic.ActiveMessagePartitionFile;

                // Initialize IdKey Index File
                string newIdFileLocation = TenantLocations.GetIdKeyIndexFile(message.Tenant, message.Product, message.Component, message.Topic, $"primary_key_{topic.ActiveMessagePartitionFile}");
                fileGate.IdKeyFileStream = new FileStream(newIdFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                fileGate.IdKeyStreamWriter = new StreamWriter(fileGate.IdKeyFileStream);
            }
        }

        public void AutoFlush(string topicKey)
        {
            topicsActiveFiles[topicKey].MessageDetailsStreamWriter.Flush();
            topicsActiveFiles[topicKey].IdKeyStreamWriter.Flush();
        }
        public void AutoFlush(MessageFileGate fileGate)
        {
            fileGate.MessageDetailsStreamWriter.FlushAsync();
            fileGate.IdKeyStreamWriter.FlushAsync();
        }
    }

    public class MessageFileGate
    {
        public FileStream MessageDetailsFileStream { get; set; }
        public StreamWriter MessageDetailsStreamWriter { get; set; }

        public FileStream IdKeyFileStream { get; set; }
        public StreamWriter IdKeyStreamWriter { get; set; }

        public int RowsCount { get; set; }

        // Processing Engine
        public ConcurrentQueue<Message> MessagesBuffer { get; set; }
        public Model.Threading.ThreadPool ThreadPool { get; set; }

        public string ActivePartitionFile { get; set; }
        public MessageFileGate(int agentSize)
        {
            ThreadPool = new Model.Threading.ThreadPool(agentSize);

            MessagesBuffer = new ConcurrentQueue<Message>();
            RowsCount = 0;
        }
    }
}
