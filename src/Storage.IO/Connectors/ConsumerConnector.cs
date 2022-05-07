using Buildersoft.Andy.X.Storage.IO.Background.Services;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.IO.Connectors
{
    public class ConsumerConnector
    {
        private readonly ILogger<ConsumerIOService> _logger;
        private readonly string _tenant;
        private readonly string _product;
        private readonly string _component;
        private readonly string _topic;
        private readonly string _consumer;
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly Timer _flushPointerTimer;

        private readonly ConsumerArchiveBackgroundService _consumerArchiveBackgroundService;

        public ConsumerPointerContext ConsumerPointerContext { get; set; }

        // Processing Engine
        public Model.Threading.ThreadPool ThreadingPool { get; set; }
        public ConcurrentQueue<Model.Entities.ConsumerMessage> MessagesBuffer { get; set; }

        private bool isMemoryReleased;

        public int Count { get; set; }
        public ConcurrentDictionary<Guid, Model.Entities.ConsumerMessage> BatchAcknowledgedConsumerMessagesToMerge { get; set; }
        public ConcurrentDictionary<Guid, Model.Entities.ConsumerMessage> BatchUnacknowledgedConsumerMessagesToMerge { get; set; }


        public ConsumerConnector(ILogger<ConsumerIOService> logger,
            string tenant,
            string product,
            string component,
            string topic,
            string consumer,
            ConsumerPointerContext consumerPointer,
            PartitionConfiguration partitionConfiguration,
            int agentCount)
        {
            _logger = logger;
            _tenant = tenant;
            _product = product;
            _component = component;
            _topic = topic;
            _consumer = consumer;

            _partitionConfiguration = partitionConfiguration;


            ConsumerPointerContext = null;
            ThreadingPool = new Model.Threading.ThreadPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Model.Entities.ConsumerMessage>();
            BatchAcknowledgedConsumerMessagesToMerge = new ConcurrentDictionary<Guid, Model.Entities.ConsumerMessage>();
            BatchUnacknowledgedConsumerMessagesToMerge = new ConcurrentDictionary<Guid, Model.Entities.ConsumerMessage>();
            ConsumerPointerContext = consumerPointer;
            Count = 0;
            isMemoryReleased = true;
            try
            {
                consumerPointer.ChangeTracker.AutoDetectChangesEnabled = false;
                consumerPointer.Database.EnsureCreated();

                // Database exists
                // Create new instance of Backend ConsumerArchiveBackgroundService
                _consumerArchiveBackgroundService = new ConsumerArchiveBackgroundService(logger, tenant, product, component, topic, consumer, partitionConfiguration, consumerPointer);
                _consumerArchiveBackgroundService.StartService();
            }
            catch (Exception)
            {

            }

            _flushPointerTimer = new Timer();
            _flushPointerTimer.Interval = partitionConfiguration.FlushInterval;
            _flushPointerTimer.Elapsed += FlushPointerTimer_Elapsed;
            _flushPointerTimer.AutoReset = true;
            _flushPointerTimer.Start();
        }

        private void FlushPointerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flushPointerTimer.Stop();

            AutoFlushAcknowledgedBatchPointers();
            AutoFlushUnacknowledgedBatchPointers();

            ReleaseMemory();

            _flushPointerTimer.Start();
        }

        private void ReleaseMemory()
        {
            if (isMemoryReleased == false)
            {
                if (MessagesBuffer.Count == 0 && BatchAcknowledgedConsumerMessagesToMerge.Count == 0 && BatchUnacknowledgedConsumerMessagesToMerge.Count == 0)
                {
                    // ConsumerPointerContext.Dispose();
                    GC.Collect();
                    GC.SuppressFinalize(this);
                    GC.SuppressFinalize(ConsumerPointerContext);
                    GC.SuppressFinalize(MessagesBuffer);
                    GC.SuppressFinalize(BatchAcknowledgedConsumerMessagesToMerge);
                    GC.SuppressFinalize(BatchUnacknowledgedConsumerMessagesToMerge);

                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();

                    isMemoryReleased = true;
                }
            }
        }

        public void EnableReleaseMemoryFlag()
        {
            if (isMemoryReleased == true)
                isMemoryReleased = false;
        }

        private void AutoFlushAcknowledgedBatchPointers()
        {
            lock (BatchAcknowledgedConsumerMessagesToMerge)
            {
                if (ThreadingPool.AreThreadsRunning == true)
                {
                    if (BatchAcknowledgedConsumerMessagesToMerge.Count >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Model.Entities.ConsumerMessage>(BatchAcknowledgedConsumerMessagesToMerge.Values);
                        ConsumerPointerContext.BulkInsertOrUpdate(batchToInsert);
                        RemoveRegisteredFromDictionary(BatchAcknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
                else
                {
                    if (BatchAcknowledgedConsumerMessagesToMerge.Count != 0)
                    {
                        var batchToInsert = new List<Model.Entities.ConsumerMessage>(BatchAcknowledgedConsumerMessagesToMerge.Values);
                        ConsumerPointerContext.BulkInsertOrUpdate(batchToInsert);
                        RemoveRegisteredFromDictionary(BatchAcknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
            }
        }

        private void RemoveRegisteredFromDictionary(ConcurrentDictionary<Guid, Model.Entities.ConsumerMessage> source, List<Model.Entities.ConsumerMessage> toRemoveList)
        {
            toRemoveList.ForEach(toRemove =>
            {
                source.TryRemove(toRemove.MessageId, out _);
            });
        }

        private void AutoFlushUnacknowledgedBatchPointers()
        {
            CheckIfPointerIsStoredAsAcknowledged();
            lock (BatchUnacknowledgedConsumerMessagesToMerge)
            {
                if (ThreadingPool.AreThreadsRunning == true)
                {
                    // Flush unacknowledged message
                    if (BatchUnacknowledgedConsumerMessagesToMerge.Count() >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Model.Entities.ConsumerMessage>(BatchUnacknowledgedConsumerMessagesToMerge.Values);
                        ConsumerPointerContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
                        RemoveRegisteredFromDictionary(BatchUnacknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
                else
                {
                    // Flush unacknowledged message
                    if (BatchUnacknowledgedConsumerMessagesToMerge.Count() != 0)
                    {
                        var batchToInsert = new List<Model.Entities.ConsumerMessage>(BatchUnacknowledgedConsumerMessagesToMerge.Values);
                        ConsumerPointerContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
                        RemoveRegisteredFromDictionary(BatchUnacknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
            }
        }

        private void CheckIfPointerIsStoredAsAcknowledged()
        {
            // check if these records are stored in db before as acked.
            var batch = BatchUnacknowledgedConsumerMessagesToMerge.Values;
            if (batch == null)
                return;

            if (batch.Count > 0)
            {
                // why this is returning alwayse zero, lets try it later...
                var recordsExist = ConsumerPointerContext
                        .ConsumerMessages.Where(x => batch.Select(a => a.MessageId)
                        .Contains(x.MessageId) && x.IsAcknowledged == true).ToList();

                recordsExist.ForEach(ex =>
                {
                    BatchUnacknowledgedConsumerMessagesToMerge.TryRemove(ex.MessageId, out _);
                });
            }
        }

        public void StopAutoFlushPointer()
        {
            _flushPointerTimer.Elapsed -= FlushPointerTimer_Elapsed;
            _flushPointerTimer.Stop();

            _consumerArchiveBackgroundService.StopService();
        }
    }
}
