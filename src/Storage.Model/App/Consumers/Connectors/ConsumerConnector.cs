using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors
{
    public class ConsumerConnector
    {
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly Timer _flushPointerTimer;

        public TenantContext TenantContext { get; set; }

        // Processing Engine
        public Threading.ThreadPool ThreadingPool { get; set; }
        public ConcurrentQueue<Entities.ConsumerMessage> MessagesBuffer { get; set; }


        public int Count { get; set; }
        public ConcurrentDictionary<Guid, Entities.ConsumerMessage> BatchAcknowledgedConsumerMessagesToMerge { get; set; }
        public ConcurrentDictionary<Guid, Entities.ConsumerMessage> BatchUnacknowledgedConsumerMessagesToMerge { get; set; }


        public ConsumerConnector(TenantContext tenantContext, PartitionConfiguration partitionConfiguration, int agentCount)
        {
            Console.WriteLine($"REMOVE THIS LINE : ConsumerConnector is initialized");


            TenantContext = null;
            ThreadingPool = new Threading.ThreadPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Entities.ConsumerMessage>();
            BatchAcknowledgedConsumerMessagesToMerge = new ConcurrentDictionary<Guid, Entities.ConsumerMessage>();
            BatchUnacknowledgedConsumerMessagesToMerge = new ConcurrentDictionary<Guid, Entities.ConsumerMessage>();

            TenantContext = tenantContext;
            _partitionConfiguration = partitionConfiguration;
            Count = 0;

            try
            {
                tenantContext.ChangeTracker.AutoDetectChangesEnabled = false;
                tenantContext.Database.EnsureCreated();
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

            _flushPointerTimer.Start();
        }

        private void AutoFlushAcknowledgedBatchPointers()
        {
            lock (BatchAcknowledgedConsumerMessagesToMerge)
            {
                if (ThreadingPool.AreThreadsRunning == true)
                {
                    if (BatchAcknowledgedConsumerMessagesToMerge.Count() >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Entities.ConsumerMessage>(BatchAcknowledgedConsumerMessagesToMerge.Values);
                        TenantContext.BulkInsertOrUpdate(batchToInsert);
                        RemoveRegisteredFromDictionary(BatchAcknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
                else
                {
                    if (BatchAcknowledgedConsumerMessagesToMerge.Count() != 0)
                    {
                        var batchToInsert = new List<Entities.ConsumerMessage>(BatchAcknowledgedConsumerMessagesToMerge.Values);
                        TenantContext.BulkInsertOrUpdate(batchToInsert);
                        RemoveRegisteredFromDictionary(BatchAcknowledgedConsumerMessagesToMerge, batchToInsert);
                    }
                }
            }
        }

        private void RemoveRegisteredFromDictionary(ConcurrentDictionary<Guid, Entities.ConsumerMessage> source, List<Entities.ConsumerMessage> toRemoveList)
        {
            toRemoveList.ForEach(toRemove =>
            {
                source.TryRemove(toRemove.MessageId, out _);
            });
        }

        private void AutoFlushUnacknowledgedBatchPointers(bool flushAnyway = false)
        {
            CheckIfPointerIsStoredAsAcknowledged();
            lock (BatchUnacknowledgedConsumerMessagesToMerge)
            {
                if (flushAnyway == false)
                {
                    // Flush unacknowledged message
                    if (BatchUnacknowledgedConsumerMessagesToMerge.Count() >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Entities.ConsumerMessage>(BatchUnacknowledgedConsumerMessagesToMerge.Values);
                        TenantContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
                        RemoveRegisteredFromDictionary(BatchUnacknowledgedConsumerMessagesToMerge, batchToInsert);

                    }
                }
                else
                {
                    // Flush unacknowledged message
                    if (BatchUnacknowledgedConsumerMessagesToMerge.Count() != 0)
                    {
                        var batchToInsert = new List<Entities.ConsumerMessage>(BatchUnacknowledgedConsumerMessagesToMerge.Values);
                        TenantContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
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
                var recordsExist = TenantContext
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
        }
    }
}
