using System;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers
{
    public class Consumer
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public SubscriptionType SubscriptionType { get; set; }

        public DateTime CreatedDate { get; set; }
        public ConsumerSettings ConsumerSettings { get; set; }
        public Consumer()
        {
            ConsumerSettings = new ConsumerSettings();
        }

    }

    public class ConsumerSettings
    {
        public InitialPosition InitialPosition { get; set; }
        public ConsumerSettings()
        {
            InitialPosition = InitialPosition.Latest;
        }
    }

    public enum SubscriptionType
    {
        /// <summary>
        /// Only one reader
        /// </summary>
        Exclusive,
        /// <summary>
        /// One reader with one backup
        /// </summary>
        Failover,
        /// <summary>
        /// Shared to more than one reader.
        /// </summary>
        Shared
    }

    public enum InitialPosition
    {
        Earliest,
        Latest
    }
}
