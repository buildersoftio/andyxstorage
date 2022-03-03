using Buildersoft.Andy.X.Storage.Model.Commands.Consumer;
using System;

namespace Buildersoft.Andy.X.Storage.Model.Commands.Producer
{
    public class NotifyProducerConnection
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }

        public Guid Id { get; set; }
        public string ProducerName { get; set; }

        public ConnectionType ConnectionType { get; set; }

    }
}
