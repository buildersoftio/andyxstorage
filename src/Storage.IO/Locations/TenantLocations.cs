﻿using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class TenantLocations
    {
        public static string GetTenantDirectory(string tenantName)
        {
            return Path.Combine(SystemLocations.GetTenantRootDirectory(), tenantName);
        }

        public static string GetTenantLogsRootDirectory(string tenantName)
        {
            return Path.Combine(GetTenantDirectory(tenantName), "logs");
        }

        public static string GetTenantTokensDirectory(string tenantName)
        {
            return Path.Combine(GetTenantDirectory(tenantName), "tokens");
        }

        public static string GetProductRootDirectory(string tenantName)
        {
            return Path.Combine(GetTenantDirectory(tenantName));
        }

        public static string GetProductDirectory(string tenantName, string productName)
        {
            return Path.Combine(GetProductRootDirectory(tenantName), productName);
        }

        public static string GetComponentRootDirectory(string tenantName, string productName)
        {
            return Path.Combine(GetProductDirectory(tenantName, productName));
        }

        public static string GetComponentDirectory(string tenantName, string productName, string componentName)
        {
            return Path.Combine(GetComponentRootDirectory(tenantName, productName), componentName);
        }

        public static string GetComponentTokenDirectory(string tenantName, string productName, string componentName)
        {
            return Path.Combine(GetComponentDirectory(tenantName, productName, componentName), "tokens");
        }

        public static string GetTopicRootDirectory(string tenantName, string productName, string componentName)
        {
            return Path.Combine(GetComponentDirectory(tenantName, productName, componentName));
        }

        public static string GetTopicDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicRootDirectory(tenantName, productName, componentName), topicName);
        }

        public static string GetProducerRootDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), "producers");
        }

        public static string GetConsumerRootDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), "consumers");
        }

        public static string GetIndexRootDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), "indexes");
        }

        public static string GetTopicLogRootDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), "logs");
        }

        public static string GetMessageRootDirectory(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), "messages");
        }

        public static string GetMessagePartitionFile(string tenantName, string productName, string componentName, string topicName, string partition)
        {
            return Path.Combine(GetMessageRootDirectory(tenantName, productName, componentName, topicName), partition);
        }

        public static string GetIdKeyIndexFile(string tenantName, string productName, string componentName, string topicName, string idKeyFile)
        {
            return Path.Combine(GetIndexRootDirectory(tenantName, productName, componentName, topicName), idKeyFile);
        }

        public static string GetTenantConfigFile(string tenantName)
        {
            return Path.Combine(GetTenantDirectory(tenantName), $"{tenantName}_config.andx");
        }

        public static string GetProductConfigFile(string tenantName, string productName)
        {
            return Path.Combine(GetProductDirectory(tenantName, productName), $"{productName}_config.andx");
        }

        public static string GetComponentConfigFile(string tenantName, string productName, string componentName)
        {
            return Path.Combine(GetComponentDirectory(tenantName, productName, componentName), $"{componentName}_config.andx");
        }

        public static string GetTopicConfigFile(string tenantName, string productName, string componentName, string topicName)
        {
            return Path.Combine(GetTopicDirectory(tenantName, productName, componentName, topicName), $"{topicName}_config.andx");
        }


        public static string GetTenantTodayLogFile(string tenantName)
        {
            return Path.Combine(GetTenantLogsRootDirectory(tenantName), $"{tenantName}_{DateTime.Now:dd.MM.yyyy}.log");
        }
    }
}
