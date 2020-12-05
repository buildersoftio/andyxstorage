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

            string logLine = $"{DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss")}\t{readerName}\tconnected";
            StoreLogInReader(bookLocation, readerName, logLine);

            return true;
        }

        public static bool StoreLogInReader(string bookLocation, string readerName, string logLine)
        {
            string readerLogFileLocation = Path.Combine(bookLocation, "Readers", readerName, $"events-{DateTime.Now.ToString("yyyy-MM")}.log");
            if (!File.Exists(readerLogFileLocation))
            {
                string header = $"Date\tReader/MessageId\tEvent{Environment.NewLine}";
                File.WriteAllText(readerLogFileLocation, $"{header}{logLine}{Environment.NewLine}");
            }
            else
                File.AppendAllText(readerLogFileLocation, $"{logLine}{Environment.NewLine}");
            return true;
        }
    }
}
