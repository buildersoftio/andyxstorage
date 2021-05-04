using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Buildersoft.Andy.X.Storage.Core.Provider
{
    public class XNodeConnectionProvider
    {
        private readonly XNodeConfiguration nodeConfig;
        private readonly DataStorageConfiguration dataStorageConfig;
        private readonly string agentIdConnection;

        private HubConnection _connection;

        public XNodeConnectionProvider(XNodeConfiguration nodeConfig,
            DataStorageConfiguration dataStorageConfig,
            string agentIdConnection)
        {
            this.nodeConfig = nodeConfig;
            this.dataStorageConfig = dataStorageConfig;
            this.agentIdConnection = agentIdConnection;
            ConnectToXNode();
        }

        private void ConnectToXNode()
        {
            BuildConnectionWithAgent(nodeConfig);
        }

        private void BuildConnectionWithAgent(XNodeConfiguration nodeConfig)
        {
            string serviceUrl = CheckAndFixServiceUrl(nodeConfig.ServiceUrl);
            _connection = new HubConnectionBuilder()
                .AddJsonProtocol(opts =>
                {
                    opts.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .WithUrl($"{serviceUrl}/realtime/v2/datastorage", option =>
                {
                    option.Headers["Authorization"] = $"Bearer {nodeConfig.JwtToken}";
                    option.Headers["x-andy-storage-name"] = dataStorageConfig.Name;
                    option.Headers["x-andy-storage-agent"] = agentIdConnection;
                })
                .WithAutomaticReconnect()
                .Build();
        }

        private string CheckAndFixServiceUrl(string serviceUrl)
        {
            if (serviceUrl.EndsWith("/"))
                return serviceUrl.Remove(serviceUrl.Length - 1);

            return serviceUrl;
        }

        public HubConnection GetHubConnection()
        {
            return _connection;
        }
    }
}
