using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants
{
    public static class ReaderConfigFile
    {
        public static bool SaveReaderConfigFile(string bookLocation, string readerName, object readerDetails)
        {
            string readerConfigFileLocation = Path.Combine(bookLocation, "Readers", $"{readerName}_config.json");
            string readerLocation = Path.Combine(bookLocation, "Readers", readerName);

            if (File.Exists(readerConfigFileLocation))
                File.Delete(readerConfigFileLocation);

            File.WriteAllText(readerConfigFileLocation, readerDetails.ToPrettyJson());

            if (!Directory.Exists(readerLocation))
                Directory.CreateDirectory(readerLocation);

            return true;
        }
    }
}
