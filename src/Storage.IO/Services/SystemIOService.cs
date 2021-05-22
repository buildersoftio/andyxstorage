using Buildersoft.Andy.X.Storage.IO.Locations;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public static class SystemIOService
    {
        public static void CreateConfigDirectories()
        {
            Directory.CreateDirectory(SystemLocations.GetDataDirectory());
            Directory.CreateDirectory(SystemLocations.GetConfigNodesDirectory());
            Directory.CreateDirectory(SystemLocations.GetConfigGeoReplicationDirectory());
            Directory.CreateDirectory(SystemLocations.GetConfigCredentialsDirectory());
            Directory.CreateDirectory(SystemLocations.GetStorageDirectory());
            Directory.CreateDirectory(SystemLocations.GetTenantRootDirectory());
        }
    }
}
