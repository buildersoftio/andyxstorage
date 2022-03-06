using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool isMemoryReleased { get; set; }

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

            isMemoryReleased = true;
        }

        private void FlushPointerTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _flushPointerTimer.Stop();

            FlushBatchToDisk();

            ReleaseMemory();

            _flushPointerTimer.Start();
        }

        private void ReleaseMemory()
        {
            if (isMemoryReleased == false)
            {
                if (MessagesBuffer.Count == 0 && BatchMessagesToInsert.Count == 0)
                {
                    // MessageContext.Dispose();
                    GC.Collect();
                    GC.SuppressFinalize(this);
                    GC.SuppressFinalize(MessageContext);
                    GC.SuppressFinalize(MessagesBuffer);
                    GC.SuppressFinalize(BatchMessagesToInsert);

                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();

                    isMemoryReleased = true;
                }
            }
        }

        private void FlushBatchToDisk()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
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

                // Enable Memory Release
                isMemoryReleased = false;
            }
            catch (Exception)
            {

            }
        }

        public void DisposeAutoFlushPointer()
        {
            _flushPointerTimer.Elapsed -= FlushPointerTimer_Elapsed;
            _flushPointerTimer.Stop();
        }
    }
}
