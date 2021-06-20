using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Consumers;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Writers
{
    public static class ConsumerWriter
    {
        public static bool WriteConsumerConfigFile(string tenant, string product, string component, string topic, Consumer consumer)
        {
            try
            {
                if (File.Exists(ConsumerLocations.GetConsumerConfigFile(tenant, product, component, topic, consumer.Name)))
                    File.Delete(ConsumerLocations.GetConsumerConfigFile(tenant, product, component, topic, consumer.Name));

                File.WriteAllText(ConsumerLocations.GetConsumerConfigFile(tenant, product, component, topic, consumer.Name), consumer.ToJsonAndEncrypt());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async static void WriteInConsumerLogFile(string tenant, string product, string component, string topic, string consumerName, string rowLog)
        {
            try
            {
                await File.AppendAllTextAsync(ConsumerLocations.GetConsumerStateWeekLogFile(tenant, product, component, topic, consumerName), $"{rowLog}\n");
            }
            catch (Exception)
            {
                // TODO: handle this exception
            }
        }
    }
}
