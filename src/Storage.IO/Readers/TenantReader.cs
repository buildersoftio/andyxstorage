using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Readers
{
    public static class TenantReader
    {
        public static Topic ReadTopicConfigFile(string tenant, string product, string component, string topic)
        {
            return File
                .ReadAllText(TenantLocations.GetTopicConfigFile(tenant, product, component, topic))
                .JsonToObjectAndDecrypt<Topic>();
        }

        public static Consumer ReadConsumerConfigFile(string tenant, string product, string component, string topic, string consumer)
        {
            return File
                .ReadAllText(ConsumerLocations.GetConsumerConfigFile(tenant, product, component, topic, consumer))
                .JsonToObjectAndDecrypt<Consumer>();
        }

        public static List<Consumer> ReadAllConsumers()
        {
            List<Consumer> consumersResult = new List<Consumer>();

            if (Directory.Exists(SystemLocations.GetTenantRootDirectory()) != true)
                return consumersResult;

            string[] tenants = Directory.GetDirectories(SystemLocations.GetTenantRootDirectory());
            foreach (var tenantLocation in tenants)
            {
                // go on every product
                string tenant = Path.GetFileName(tenantLocation);
                string[] products = Directory.GetDirectories(TenantLocations.GetProductRootDirectory(tenant));
                foreach (var productLocation in products)
                {
                    string product = Path.GetFileName(productLocation);
                    if (product == "logs")
                        continue;

                    string[] components = Directory.GetDirectories(TenantLocations.GetComponentRootDirectory(tenant, product));
                    foreach (var componentLocation in components)
                    {
                        string component = Path.GetFileName(componentLocation);

                        string[] topics = Directory.GetDirectories(TenantLocations.GetTopicRootDirectory(tenant, product, component));
                        foreach (var topicLocation in topics)
                        {
                            string topic = Path.GetFileName(topicLocation);
                            string[] consumers = Directory.GetDirectories(TenantLocations.GetConsumerRootDirectory(tenant, product, component, topic));
                            foreach (var consumerLocation in consumers)
                            {
                                string consumer = Path.GetFileName(consumerLocation);
                                consumersResult.Add(ReadConsumerConfigFile(tenant, product, component, topic, consumer));
                            }
                        }
                    }
                }
            }

            return consumersResult;
        }
    }
}
