using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.App.Topics;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
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
    }
}
