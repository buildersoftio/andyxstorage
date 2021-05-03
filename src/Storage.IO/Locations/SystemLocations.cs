using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Locations
{
    public static class SystemLocations
    {

        #region Directories
        public static string GetRootDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetDataDirectory()
        {
            return Path.Combine(GetRootDirectory(), "data");
        }

        public static string GetConfigDirectory()
        {
            return Path.Combine(GetRootDirectory(), "data", "config");
        }

        public static string GetConfigNodesDirectory()
        {
            return Path.Combine(GetConfigDirectory(), "nodes");
        }

        public static string GetConfigGeoReplicationDirectory()
        {
            return Path.Combine(GetConfigDirectory(), "geo-replication");
        }

        public static string GetConfigCredentialsDirectory()
        {
            return Path.Combine(GetConfigDirectory(), "credentials");
        }

        public static string GetStorageDirectory()
        {
            return Path.Combine(GetRootDirectory(), "data", "storage");
        }
        #endregion

        #region Configuration Files
        public static string GetNodesConfigFile()
        {
            return Path.Combine(GetConfigNodesDirectory(), "andyx-nodes.andx");
        }

        public static string GetConfigGeoReplicationFile()
        {
            return Path.Combine(GetConfigGeoReplicationDirectory(), "replication.geo.andx");
        }

        public static string GetCredentialsConfigFile()
        {
            return Path.Combine(GetConfigCredentialsDirectory(), "default-user.andx");
        }

        #endregion
    }
}
