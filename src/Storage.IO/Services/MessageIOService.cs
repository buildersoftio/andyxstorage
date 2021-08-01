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

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class MessageIOService
    {
        private readonly ILogger<MessageIOService> logger;
        private readonly PartitionConfiguration partitionConfiguration;
        private ConcurrentDictionary<string, MessageFileGate> topicsActiveFiles;

        public MessageIOService(ILogger<MessageIOService> logger, PartitionConfiguration partitionConfiguration)
        {
            this.logger = logger;
            this.partitionConfiguration = partitionConfiguration;

            topicsActiveFiles = new ConcurrentDictionary<string, MessageFileGate>();
        }

        private void InitializeMessagingProcessor(string topicKey)
        {
            if (topicsActiveFiles[topicKey].IsProcessorWorking != true)
            {
                topicsActiveFiles[topicKey].IsProcessorWorking = true;
                new Thread(() => MessagingProcessor(topicKey)).Start();
            }
        }

        private void MessagingProcessor(string topicKey)
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

                        // We are commenting the logging for now, in the future logging will be written into log files, in console we will show only mandatory logs and errors.
                        //logger.LogInformation($"ANDYX-STORAGE#MESSAGES|{message.Tenant}|{message.Product}|{message.Component}|{message.Topic}|msg-{message.Id}|partition_index:{topicsActiveFiles[topicKey].RowsCount}|STORED");
                    }
                    else
                        logger.LogError($"ANDYX-STORAGE#MESSAGES|ERROR|Processing of message failed, couldn't Dequeue.|TOPIC|{topicKey}");
                    // Increase the Counter
                }
                catch (Exception)
                {

                }
            }
            topicsActiveFiles[topicKey].IsProcessorWorking = false;

            // Flush all messages to disk
            topicsActiveFiles[topicKey].MessageDetailsStreamWriter.Flush();
            topicsActiveFiles[topicKey].IdKeyStreamWriter.Flush();
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

                // Initialize IdKey Index File
                string newIdFileLocation = TenantLocations.GetIdKeyIndexFile(message.Tenant, message.Product, message.Component, message.Topic, $"primary_key_{topic.ActiveMessagePartitionFile}");
                topicsActiveFiles[topicKey].IdKeyFileStream = new FileStream(newIdFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                topicsActiveFiles[topicKey].IdKeyStreamWriter = new StreamWriter(topicsActiveFiles[topicKey].IdKeyFileStream);
            }

            // Check if the file has 5k lines
            CheckAndChangePartition(message, topicsActiveFiles[topicKey]);

            // Write message
            topicsActiveFiles[topicKey].MessageDetailsStreamWriter.WriteLine(new MessageRow() { Id = message.Id, MessageRaw = message.MessageRaw }.ToJsonAndEncrypt());
            topicsActiveFiles[topicKey].IdKeyStreamWriter.WriteLine(message.Id);

            // Flushing to disk every 100 messages
            if (topicsActiveFiles[topicKey].RowsCount % 100 == 0)
            {
                topicsActiveFiles[topicKey].MessageDetailsStreamWriter.Flush();
                topicsActiveFiles[topicKey].IdKeyStreamWriter.Flush();
            }
        }

        public void WriteMessageInFile(Message message)
        {
            string topicKey = $"{message.Tenant}-{message.Product}-{message.Component}-{message.Topic}";
            if (topicsActiveFiles.ContainsKey(topicKey) != true)
            {
                var fileGate = new MessageFileGate()
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

            if (fileGate.RowsCount >= partitionConfiguration.Size)
            {
                // Close connection with current partition

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

                // Initialize IdKey Index File
                string newIdFileLocation = TenantLocations.GetIdKeyIndexFile(message.Tenant, message.Product, message.Component, message.Topic, $"primary_key_{topic.ActiveMessagePartitionFile}");
                fileGate.IdKeyFileStream = new FileStream(newIdFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                fileGate.IdKeyStreamWriter = new StreamWriter(fileGate.IdKeyFileStream);
            }
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
        public bool IsProcessorWorking { get; set; }

        public MessageFileGate()
        {
            MessagesBuffer = new ConcurrentQueue<Message>();
            IsProcessorWorking = false;
            RowsCount = 0;
        }
    }
}
