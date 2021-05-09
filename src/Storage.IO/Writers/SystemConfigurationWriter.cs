using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Buildersoft.Andy.X.Storage.Utility.Extensions.Json;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Writers
{
    public static class SystemConfigurationWriter
    {
        public static bool WriteXNodesConfigurationFromFile(List<XNodeConfiguration> xNodes)
        {
            if (File.Exists(SystemLocations.GetNodesConfigFile()))
                File.Delete(SystemLocations.GetNodesConfigFile());
            try
            {
                File.WriteAllText(SystemLocations.GetNodesConfigFile(), xNodes.ToJsonAndEncrypt());
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool WriteStorageConfigurationFromFile(DataStorageConfiguration storageConfiguration)
        {
            if (File.Exists(SystemLocations.GetStorageCredentialsConfigFile()))
                File.Delete(SystemLocations.GetStorageCredentialsConfigFile());
            try
            {
                File.WriteAllText(SystemLocations.GetStorageCredentialsConfigFile(), storageConfiguration.ToJsonAndEncrypt());
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static bool WriteCredentialsConfigurationFromFile(CredentialsConfiguration credentialsConfiguration)
        {
            if (File.Exists(SystemLocations.GetCredentialsConfigFile()))
                File.Delete(SystemLocations.GetCredentialsConfigFile());
            try
            {
                File.WriteAllText(SystemLocations.GetCredentialsConfigFile(), credentialsConfiguration.ToJsonAndEncrypt());
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}
