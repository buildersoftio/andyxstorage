using Buildersoft.Andy.X.Storage.Model.Contexts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors
{
    public class ConsumerConnector
    {
        public TenantContext TenantContext { get; set; }
        // Processing Engine
        public ConcurrentQueue<Entities.ConsumerMessage> MessagesBuffer { get; set; }
        public bool IsProcessorWorking { get; set; }
        public int Count { get; set; }

        public ConcurrentDictionary<Guid, Entities.ConsumerMessage> BatchConsumerMessagesToInsert { get; set; }
        public ConcurrentDictionary<Guid, Entities.ConsumerMessage> BatchConsumerMessagesToUpdate { get; set; }

        public ConsumerConnector(TenantContext tenantContext)
        {
            TenantContext = null;
            IsProcessorWorking = false;
            MessagesBuffer = new ConcurrentQueue<Entities.ConsumerMessage>();
            BatchConsumerMessagesToInsert = new ConcurrentDictionary<Guid, Entities.ConsumerMessage>();
            BatchConsumerMessagesToUpdate = new ConcurrentDictionary<Guid, Entities.ConsumerMessage>();

            TenantContext = tenantContext;
            Count = 0;

            try
            {
                tenantContext.Database.EnsureCreated();
            }
            catch (System.Exception)
            {

            }
        }
    }
}
