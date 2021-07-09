using Buildersoft.Andy.X.Storage.Model.Contexts;
using Buildersoft.Andy.X.Storage.Model.Entities;
using System.Collections.Concurrent;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers.Connectors
{
    public class ConsumerConnector
    {
        public TenantContext TenantContext { get; set; }
        // Processing Engine
        public ConcurrentQueue<ConsumerMessage> MessagesBuffer { get; set; }
        public bool IsProcessorWorking { get; set; }
        public int Count { get; set; }

        public ConsumerConnector(TenantContext tenantContext)
        {
            TenantContext = null;
            IsProcessorWorking = false;
            MessagesBuffer = new ConcurrentQueue<ConsumerMessage>();
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
