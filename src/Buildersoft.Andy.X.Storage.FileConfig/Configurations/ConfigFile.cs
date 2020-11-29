using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Buildersoft.Andy.X.Storage.FileConfig.Configurations
{
    public static class ConfigFile
    {
        public static DataStorage GetDataStorageSettings()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "dataStorage_config.json");
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations"));

            if (!File.Exists(filePath))
                return new DataStorage();

            return File.ReadAllText(filePath).JsonToObject<DataStorage>();
        }

        public static bool UpdateDataStorageSettings(DataStorage dataStorage)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "dataStorage_config.json");
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations"));

            if (dataStorage != null)
                File.WriteAllText(filePath, dataStorage.ToPrettyJson());
            else
                throw new Exception("dataStorage settings can not be null");

            return true;
        }

        public static bool IsDataStorageConfiged()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "dataStorage_config.json");
            if (File.Exists(filePath)) // Check if data inside the file are DataStorage Object
                return true;

            return false;
        }

        // ANDY X
        public static AndyXProperty GetAndyXSettings()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "andyx_config.json");
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations"));

            if (!File.Exists(filePath))
                return new AndyXProperty();

            return File.ReadAllText(filePath).JsonToObject<AndyXProperty>();
        }

        public static bool UpdateAndyXSettings(AndyXProperty dataStorage)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations", "andyx_config.json");
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations")))
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configurations"));

            if (dataStorage != null)
                File.WriteAllText(filePath, dataStorage.ToPrettyJson());
            else
                throw new Exception("Andy X settings can not be null");

            return true;
        }

    }
}
