using Buildersoft.Andy.X.Storage.Data.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model
{
    public class DataStorage
    {
        //TODO... Add Data that we need to store for SignalR Clients on DataStorages
        public Guid DataStoregeId { get; private set; }
        public string DataStorageName { get; set; }
        public DataStorageEnvironment DataStorageEnvironment { get; set; }
        public DataStorageType DataStorageType { get; set; }
        public DataStorageStatus DataStorageStatus { get; set; }
        public DataStorage()
        {
            DataStoregeId = Guid.NewGuid();
            DataStorageName = "";
        }
    }
}
