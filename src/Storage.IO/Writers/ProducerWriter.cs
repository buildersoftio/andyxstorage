using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Producers;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Writers
{
    public static class ProducerWriter
    {
        public static bool WriteProducerConfigFile(string tenant, string product, string component, string topic, Producer producer)
        {
            try
            {
                if (File.Exists(ProducerLocations.GetProducerConfigFile(tenant, product, component, topic, producer.Name)))
                    File.Delete(ProducerLocations.GetProducerConfigFile(tenant, product, component, topic, producer.Name));

                File.WriteAllText(ProducerLocations.GetProducerConfigFile(tenant, product, component, topic, producer.Name), producer.ToJsonAndEncrypt());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async static void WriteInProducerLogFile(string tenant, string product, string component, string topic, string producerName, string rowLog)
        {
            try
            {
                await File.AppendAllTextAsync(ProducerLocations.GetProducerStateWeekLogFile(tenant, product, component, topic, producerName), $"{rowLog}\n");
            }
            catch (Exception)
            {
                // TODO: handle this exception
            }
        }
    }
}
