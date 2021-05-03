using System.IO;

namespace Storage.IO.Locations
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
        }
    }
}
