using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.App.Messages;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class MessageIOService
    {
        private readonly ILogger<MessageIOService> logger;
        private ConcurrentDictionary<string, FileGate> topicsActiveFiles;

        public MessageIOService(ILogger<MessageIOService> logger)
        {
            this.logger = logger;
            topicsActiveFiles = new ConcurrentDictionary<string, FileGate>();
        }

        private void InitializeMessagingProcessor(string topicKey)
        {
            if (topicsActiveFiles[topicKey].IsMessagingWorking != true)
            {
                topicsActiveFiles[topicKey].IsMessagingWorking = true;
                new Thread(() => MessagingProcessor(topicKey)).Start();
            }
        }

        private void MessagingProcessor(string topicKey)
        {
            while (topicsActiveFiles[topicKey].MessagesQueue.Count > 0)
            {
                try
                {
                    Message message;
                    bool isMessageReturned = topicsActiveFiles[topicKey].MessagesQueue.TryDequeue(out message);
                    if (isMessageReturned == true)
                    {
                        ProcessMessageToFileFile(topicKey, message);
                        topicsActiveFiles[topicKey].RowsCount++;
                        logger.LogInformation($"ANDYX-STORAGE#MESSAGES|{message.Tenant}|{message.Product}|{message.Component}|{message.Topic}|msg-{message.Id}|index:{topicsActiveFiles[topicKey].RowsCount}|STORED");
                    }
                    else
                        logger.LogError($"ANDYX-STORAGE#MESSAGES|ERROR|Processing of message failed, couldn't Dequeue.|TOPIC|{topicKey}");
                    // Increase the Counter
                }
                catch (Exception)
                {

                }

            }
            topicsActiveFiles[topicKey].IsMessagingWorking = false;
        }

        private void ProcessMessageToFileFile(string topicKey, Message message)
        {
            if (topicsActiveFiles[topicKey].FileStream == null)
            {
                // will create a new FileStream
                Topic topic = TenantReader.ReadTopicConfigFile(message.Tenant, message.Product, message.Component, message.Topic);
                string newFileLocation = TenantLocations.GetMessagePartitionFile(message.Tenant, message.Product, message.Component, message.Topic, topic.ActiveMessagePartitionFile);
                int countRowsInPartition = File.ReadAllLines(newFileLocation).Length;

                topicsActiveFiles[topicKey].FileStream =
                    new FileStream(newFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                topicsActiveFiles[topicKey].RowsCount = countRowsInPartition;

                topicsActiveFiles[topicKey].StreamWriter = new StreamWriter(topicsActiveFiles[topicKey].FileStream);
                topicsActiveFiles[topicKey].StreamWriter.AutoFlush = true;
            }

            // Check if the file has 5k lines
            CheckAndChangePartition(message, topicsActiveFiles[topicKey]);

            // Write message
            topicsActiveFiles[topicKey].StreamWriter.WriteLine(new MessageRow() { Id = message.Id, MessageRaw = message.MessageRaw }.ToJsonAndEncrypt());
            topicsActiveFiles[topicKey].StreamWriter.Flush();
        }

        public void WriteMessageInFile(Message message)
        {
            string topicKey = $"{message.Tenant}-{message.Product}-{message.Component}-{message.Topic}";
            if (topicsActiveFiles.ContainsKey(topicKey) != true)
            {
                var fileGate = new FileGate()
                {
                    FileStream = null,
                    RowsCount = -1
                };

                topicsActiveFiles.TryAdd(topicKey, fileGate);
            }

            topicsActiveFiles[topicKey].MessagesQueue.Enqueue(message);
            InitializeMessagingProcessor(topicKey);
        }

        private void CheckAndChangePartition(Message message, FileGate fileGate)
        {

            if (fileGate.RowsCount >= 3000)
            {
                // Close connection with current partition
                fileGate.StreamWriter.Close();
                fileGate.FileStream.Close();

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

                fileGate.FileStream = new FileStream(newFileLocation, FileMode.Append, FileAccess.Write, FileShare.None);
                fileGate.RowsCount = 0;
                fileGate.StreamWriter = new StreamWriter(fileGate.FileStream);
            }
        }
    }

    public class FileGate
    {
        public FileStream FileStream { get; set; }
        public StreamWriter StreamWriter { get; set; }
        public int RowsCount { get; set; }

        // Processing Engine
        public ConcurrentQueue<Message> MessagesQueue { get; set; }
        public bool IsMessagingWorking { get; set; }

        public FileGate()
        {
            MessagesQueue = new ConcurrentQueue<Message>();
            IsMessagingWorking = false;
            RowsCount = 0;
        }
    }
}
