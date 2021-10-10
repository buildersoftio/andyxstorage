using Buildersoft.Andy.X.Storage.IO.Locations;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class SystemIOService
    {
        public SystemIOService(ILogger<SystemIOService> logger)
        {
            var generalColor = Console.ForegroundColor;
            Console.WriteLine("                   Starting Buildersoft Andy X Storage");
            Console.WriteLine("                   Copyright (C) 2021 Buildersoft LLC");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("  ###"); Console.ForegroundColor = generalColor; Console.WriteLine("      ###");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("    ###"); Console.ForegroundColor = generalColor; Console.Write("  ###");
            Console.WriteLine("       Andy X Storage 2.0.3-preview. Copyright (C) 2021 Buildersoft LLC");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("      ####         "); Console.ForegroundColor = generalColor; Console.WriteLine("Licensed under the Apache License 2.0.  See https://bit.ly/3DqVQbx");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("    ###  ###");
            Console.Write("  ###      ###     "); Console.ForegroundColor = generalColor; Console.WriteLine("Andy X Storage is an open-source standalone service that is used to store messages for Andy X. The Storage service is offers support for Multitenancy storage. Andy X Storage hosts all messages and makes sure that all of them are readable for the client");
            Console.WriteLine("");


            Console.WriteLine("                   Starting Buildersoft Andy X Storage...");
            Console.WriteLine("\n");
            logger.LogInformation("ANDYX-STORAGE#READY");
        }

        public void CreateConfigDirectories()
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
