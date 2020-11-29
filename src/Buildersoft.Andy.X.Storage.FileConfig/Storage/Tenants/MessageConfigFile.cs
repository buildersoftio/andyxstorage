using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants
{
    public static class MessageConfigFile
    {
        public static bool SaveMessageFile(string fragmentId, string bookLocation, string msgId, object message)
        {
            //string fileLocation = $"{bookLocation}\\Messages\\{msgId}.json";
            string fileLocation = Path.Combine(bookLocation, $"Messages_{fragmentId}", $"{msgId}.json");
            if (File.Exists(fileLocation))
                return false;

            File.WriteAllText(fileLocation, message.ToPrettyJson());
            return true;
        }
    }
}
