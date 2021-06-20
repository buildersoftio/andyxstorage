using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class ConsumerLocations
    {
        public static string GetConsumerDirectory(string tenantName, string productName, string componentName, string topicName, string consumerName)
        {
            return Path.Combine(TenantLocations.GetConsumerRootDirectory(tenantName, productName, componentName, topicName), consumerName);
        }

        public static string GetConsumerLogsDirectory(string tenantName, string productName, string componentName, string topicName, string consumerName)
        {
            return Path.Combine(GetConsumerDirectory(tenantName, productName, componentName, topicName, consumerName), "logs");
        }

        public static string GetConsumerConfigFile(string tenantName, string productName, string componentName, string topicName, string consumerName)
        {
            return Path.Combine(GetConsumerDirectory(tenantName, productName, componentName, topicName, consumerName), $"{consumerName}_config.andx");
        }

        public static string GetConsumerStateWeekLogFile(string tenantName, string productName, string componentName, string topicName, string consumerName)
        {
            return Path.Combine(GetConsumerLogsDirectory(tenantName, productName, componentName, topicName, consumerName), $"{consumerName}_state_{DateTime.Now:MM.yyyy}.log");
        }
    }
}
