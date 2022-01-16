using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class ProducerLocations
    {
        public static string GetProducerDirectory(string tenantName, string productName, string componentName, string topicName, string producerName)
        {
            return Path.Combine(TenantLocations.GetProducerRootDirectory(tenantName, productName, componentName, topicName), producerName);
        }

        public static string GetProducerLogsDirectory(string tenantName, string productName, string componentName, string topicName, string producerName)
        {
            return Path.Combine(GetProducerDirectory(tenantName, productName, componentName, topicName, producerName), "logs");
        }
        
        public static string GetProducerConfigFile(string tenantName, string productName, string componentName, string topicName, string producerName)
        {
            return Path.Combine(GetProducerDirectory(tenantName, productName, componentName, topicName, producerName), $"{producerName}_config.andx");
        }
        
        public static string GetProducerStateWeekLogFile(string tenantName, string productName, string componentName, string topicName, string producerName)
        {
            return Path.Combine(GetProducerLogsDirectory(tenantName, productName, componentName, topicName, producerName), $"{producerName}_state_{DateTime.Now:MM.yyyy}.log");
        }
    }
}
