namespace Buildersoft.Andy.X.Storage.Model.Configuration
{
    public class DataStorageConfiguration
    {
        public string Name { get; set; }
        public DataStorageStatus Status { get; set; }
    }

    public enum DataStorageStatus
    {
        Active = 1,
        Inactive = 2,
        Blocked = 3
    }
}
