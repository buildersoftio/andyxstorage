using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.IO.Connectors
{
    public class MessageStorageConnector
    {
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly Timer _flushPointerTimer;

        public MessageContext MessageContext { get; set; }

        // Processing Engine
        public Model.Threading.ThreadPool ThreadingPool { get; set; }
        public ConcurrentQueue<Model.Entities.Message> MessagesBuffer { get; set; }
        public ConcurrentDictionary<Guid, Model.Entities.Message> BatchMessagesToInsert { get; set; }

        public MessageStorageConnector(PartitionConfiguration partitionConfiguration, int agentCount)
        {
            _partitionConfiguration = partitionConfiguration;

            MessageContext = null;
            ThreadingPool = new Model.Threading.ThreadPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Model.Entities.Message>();
            BatchMessagesToInsert = new ConcurrentDictionary<Guid, Model.Entities.Message>();

            _flushPointerTimer = new Timer();
            _flushPointerTimer.Interval = partitionConfiguration.FlushInterval;
            _flushPointerTimer.Elapsed += FlushPointerTimer_Elapsed;
            _flushPointerTimer.AutoReset = true;
            _flushPointerTimer.Start();
        }

        private void FlushPointerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flushPointerTimer.Stop();

            FlushBatchToDisk();

            _flushPointerTimer.Start();
        }

        private void FlushBatchToDisk()
        {
            try
            {
                if (ThreadingPool.AreThreadsRunning == true)
                {
                    if (BatchMessagesToInsert.Count >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Model.Entities.Message>(BatchMessagesToInsert.Values);
                        MessageContext.BulkInsert(batchToInsert);
                        RemoveRegisteredFromBatch(batchToInsert);
                    }
                }
                else
                {
                    if (BatchMessagesToInsert.Count != 0)
                    {
                        var batchToInsert = new List<Model.Entities.Message>(BatchMessagesToInsert.Values);
                        MessageContext.BulkInsert(batchToInsert);
                        RemoveRegisteredFromBatch(batchToInsert);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void RemoveRegisteredFromBatch(List<Model.Entities.Message> batchToInsert)
        {
            batchToInsert.ForEach(x =>
            {
                BatchMessagesToInsert.TryRemove(x.MessageId, out _);
            });
        }

        public void CreateMessageFile()
        {
            try
            {
                MessageContext.ChangeTracker.AutoDetectChangesEnabled = false;
                MessageContext.Database.EnsureCreated();
            }
            catch (Exception)
            {

            }
        }

        public void DisposeAutoFlushPointer()
        {
            _flushPointerTimer.Elapsed -= FlushPointerTimer_Elapsed;
            _flushPointerTimer.Stop();

            // Cleanup memory.
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

}
