using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class MessageLocations
    {
        public static string GetMessagePartitionFile(string tenantName, string productName, string componentName, string topicName, DateTime date)
        {
            return Path.Combine(TenantLocations.GetMessageRootDirectory(tenantName, productName, componentName, topicName), $"msg_part_{date:yyyy_MM_dd_HH}.xandy");
        }
    }
}
