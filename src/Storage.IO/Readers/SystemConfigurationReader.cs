using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Readers
{
    public static class SystemConfigurationReader
    {
        public static List<XNodeConfiguration> ReadXNodesConfigurationFromFile()
        {
            return File.ReadAllText(SystemLocations.GetNodesConfigFile()).JsonToObjectAndDecrypt<List<XNodeConfiguration>>();
        }

        public static DataStorageConfiguration ReadStorageConfigurationFromFile()
        {
            return File.ReadAllText(SystemLocations.GetStorageCredentialsConfigFile()).JsonToObjectAndDecrypt<DataStorageConfiguration>();
        }

        public static CredentialsConfiguration ReadCredentialsConfigurationFromFile()
        {
            return File.ReadAllText(SystemLocations.GetCredentialsConfigFile()).JsonToObjectAndDecrypt<CredentialsConfiguration>();
        }
    }
}
