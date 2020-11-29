using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.FileConfig.Configurations;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Providers
{
    public class HubConnectionProvider
    {
        private readonly ILogger<HubConnectionProvider> _logger;
        private HubConnection _connection;
        public HubConnectionProvider(ILogger<HubConnectionProvider> logger)
        {
            _logger = logger;
            DataStorage dataStorage = GetDataStorageData();
            AndyXProperty andyXProperty = GetAndyXPropertyData();
            if (dataStorage.DataStorageName != "" && andyXProperty.Name != "")
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"{andyXProperty.Url}/realtime/v1/datastorage", option =>
                    {
                        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                        if (env == "Development")
                        {
                            option.HttpMessageHandlerFactory = (message) =>
                            {
                                if (message is HttpClientHandler httpClientHandler)
                                    httpClientHandler.ServerCertificateCustomValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
                                return message;
                            };
                        }

                        option.Headers["Authorization"] = $"Bearer {andyXProperty.Token}";
                        option.Headers["x-andy-storage-name"] = dataStorage.DataStorageName;
                        option.Headers["x-andy-storage-environment"] = dataStorage.DataStorageEnvironment.ToString();
                        option.Headers["x-andy-storage-type"] = dataStorage.DataStorageType.ToString();
                        option.Headers["x-andy-storage-status"] = dataStorage.DataStorageStatus.ToString();
                    })
                    .Build();
            }
            else
                _logger.Log(LogLevel.Error, "You can not connect to a remote Andy X, configure Andy X Storage and Andy X Remote");
        }

        private AndyXProperty GetAndyXPropertyData()
        {
            return ConfigFile.GetAndyXSettings();
        }

        private DataStorage GetDataStorageData()
        {
            return ConfigFile.GetDataStorageSettings();
        }

        public HubConnection GetHubConnection()
        {
            return _connection;
        }
    }
}
