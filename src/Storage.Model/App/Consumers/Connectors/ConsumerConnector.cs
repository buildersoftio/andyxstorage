using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using System;
using System.Collections.Concurrent;
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
            _flushPointerTimer.Interval = new TimeSpan(0, 0, 5).TotalMilliseconds;
            _flushPointerTimer.Elapsed += FlushPointerTimer_Elapsed;
            _flushPointerTimer.AutoReset = true;
            _flushPointerTimer.Start();
        }

        private void FlushPointerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flushPointerTimer.Stop();

            lock (this)
            {
                AutoFlushAcknowledgedBatchPointers();
                AutoFlushUnacknowledgedBatchPointers();
            }

            _flushPointerTimer.Start();
        }

        private void AutoFlushAcknowledgedBatchPointers()
        {
            if (ThreadingPool.AreThreadsRunning == true)
            {
                Console.WriteLine($"REMOVE THIS LINE:  Batch Pointer Threads are working.... SIZE BatchAcknowledgedConsumerMessagesToMerge.Count()={BatchAcknowledgedConsumerMessagesToMerge.Count()}");
                if (BatchAcknowledgedConsumerMessagesToMerge.Count() >= _partitionConfiguration.Size)
                {
                    Console.WriteLine("REMOVE THIS LINE:  Batch Pointer Threads are working, storing acked messages");
                    TenantContext.BulkInsertOrUpdate(BatchAcknowledgedConsumerMessagesToMerge.Values.ToList());
                    BatchAcknowledgedConsumerMessagesToMerge.Clear();
                }
            }
            else
            {
                Console.WriteLine($"REMOVE THIS LINE:  Batch Pointer Threads are not working.... SIZE BatchAcknowledgedConsumerMessagesToMerge.Count()={BatchAcknowledgedConsumerMessagesToMerge.Count()}");
                if (BatchAcknowledgedConsumerMessagesToMerge.Count() != 0)
                {
                    Console.WriteLine("REMOVE THIS LINE:  Batch Pointer Threads are not working, storing acked messages");
                    TenantContext.BulkInsertOrUpdate(BatchAcknowledgedConsumerMessagesToMerge.Values.ToList());
                    BatchAcknowledgedConsumerMessagesToMerge.Clear();
                }
            }
        }

        private void AutoFlushUnacknowledgedBatchPointers(bool flushAnyway = false)
        {
            CheckIfPointerIsStoredAsAcknowledged();
            Console.WriteLine($"LINE TO REMOVE: Records are unacked before message processed : {BatchUnacknowledgedConsumerMessagesToMerge.Count()}");

            if (flushAnyway == false)
            {
                // Flush unacknowledged message
                if (BatchUnacknowledgedConsumerMessagesToMerge.Count() >= _partitionConfiguration.Size)
                {
                    TenantContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
                    BatchUnacknowledgedConsumerMessagesToMerge.Clear();
                }
            }
            else
            {
                // Flush unacknowledged message
                if (BatchUnacknowledgedConsumerMessagesToMerge.Count() != 0)
                {
                    TenantContext.BulkInsertOrUpdate(BatchUnacknowledgedConsumerMessagesToMerge.Values.ToList());
                    BatchUnacknowledgedConsumerMessagesToMerge.Clear();
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
                        .Contains(x.MessageId)).ToList();

                Console.WriteLine($"LINE TO REMOVE: >>>>>>>> Records are acked before message processed : {recordsExist.Count()}");

                recordsExist.ForEach(ex =>
                {
                    if (ex.IsAcknowledged == true)
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
