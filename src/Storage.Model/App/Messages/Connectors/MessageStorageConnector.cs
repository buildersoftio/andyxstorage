using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Model.Contexts;
using EFCore.BulkExtensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Buildersoft.Andy.X.Storage.Model.App.Messages.Connectors
{
    public class MessageStorageConnector
    {
        private readonly PartitionConfiguration _partitionConfiguration;
        private readonly Timer _flushPointerTimer;

        public MessageContext MessageContext { get; set; }

        // Processing Engine
        public Threading.ThreadPool ThreadingPool { get; set; }
        public ConcurrentQueue<Entities.Message> MessagesBuffer { get; set; }
        public ConcurrentDictionary<Guid, Entities.Message> BatchMessagesToInsert { get; set; }


        public MessageStorageConnector(MessageContext messageContext, PartitionConfiguration partitionConfiguration, int agentCount)
        {
            _partitionConfiguration = partitionConfiguration;

            MessageContext = null;
            ThreadingPool = new Threading.ThreadPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Entities.Message>();
            BatchMessagesToInsert = new ConcurrentDictionary<Guid, Entities.Message>();


            MessageContext = messageContext;

            try
            {
                messageContext.ChangeTracker.AutoDetectChangesEnabled = false;
                messageContext.Database.EnsureCreated();
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

        public MessageStorageConnector(PartitionConfiguration partitionConfiguration, int agentCount)
        {
            Console.WriteLine($"REMOVE THIS LINE : MessageStorageConnector is initialized");

            _partitionConfiguration = partitionConfiguration;

            MessageContext = null;
            ThreadingPool = new Threading.ThreadPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Entities.Message>();
            BatchMessagesToInsert = new ConcurrentDictionary<Guid, Entities.Message>();

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
                Console.WriteLine($"REMOVE THIS LINE : FLUSHING TO DISK :  BatchMessagesToInsert.Count()={BatchMessagesToInsert.Count()}, MessagesBuffer.Count={MessagesBuffer.Count()}");
                if (ThreadingPool.AreThreadsRunning == true)
                {
                    if (BatchMessagesToInsert.Count() >= _partitionConfiguration.SizeInMemory)
                    {
                        var batchToInsert = new List<Entities.Message>(BatchMessagesToInsert.Values);
                        MessageContext.BulkInsert(batchToInsert);
                        RemoveRegisteredFromBatch(batchToInsert);
                    }
                }
                else
                {
                    if (BatchMessagesToInsert.Count() != 0)
                    {
                        var batchToInsert = new List<Entities.Message>(BatchMessagesToInsert.Values);
                        MessageContext.BulkInsert(batchToInsert);
                        RemoveRegisteredFromBatch(batchToInsert);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }

        private void RemoveRegisteredFromBatch(List<Entities.Message> batchToInsert)
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

        public void StopAutoFlushPointer()
        {
            _flushPointerTimer.Elapsed -= FlushPointerTimer_Elapsed;
            _flushPointerTimer.Stop();

        }
    }
}
