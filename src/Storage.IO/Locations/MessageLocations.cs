using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class MessageLocations
    {
        public static string[] GetPartitionFiles(string tenant, string product, string component, string topic)
        {
            string[] partitionFiles = Directory.GetFiles(TenantLocations.GetMessageRootDirectory(tenant, product, component, topic));
            return partitionFiles;
        }
    }
}
