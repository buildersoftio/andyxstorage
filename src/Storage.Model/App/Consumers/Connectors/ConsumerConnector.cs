using Buildersoft.Andy.X.Storage.Model.Contexts;
using System;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors
{
    public class ConsumerConnector
    {
        public TenantContext TenantContext { get; set; }

        // Processing Engine
        public Threading.ThreadingPool ThreadingPool { get; set; }
        public ConcurrentQueue<Entities.ConsumerMessage> MessagesBuffer { get; set; }

        public bool IsProcessorWorking { get; set; }
        public int Count { get; set; }

        public ConcurrentDictionary<Guid, Entities.ConsumerMessage> BatchConsumerMessagesToMerge { get; set; }

        public ConsumerConnector(TenantContext tenantContext, int agentCount)
        {
            TenantContext = null;
            IsProcessorWorking = false;

            ThreadingPool = new Threading.ThreadingPool(agentCount);

            MessagesBuffer = new ConcurrentQueue<Entities.ConsumerMessage>();
            BatchConsumerMessagesToMerge = new ConcurrentDictionary<Guid, Entities.ConsumerMessage>();

            TenantContext = tenantContext;
            Count = 0;

            try
            {
                tenantContext.ChangeTracker.AutoDetectChangesEnabled = false;
                tenantContext.Database.EnsureCreated();
            }
            catch (System.Exception)
            {

            }
        }
    }
}
