using System;
using System.Collections.Generic;

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

        // we are adding a list of consumer connection, is needed for failover and shared subscriptions
        public List<Guid> Connections { get; set; }

        public DateTime CreatedDate { get; set; }
        public ConsumerSettings ConsumerSettings { get; set; }

        public StorageState StorageStateProperty { get; set; }

        public Consumer()
        {
            ConsumerSettings = new ConsumerSettings();
            Connections = new List<Guid>();


            StorageStateProperty = new StorageState();
        }
    }

    public class StorageState
    {
        public bool IsNewConsumer { get; set; }

        public StorageState()
        {
            IsNewConsumer = false;
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