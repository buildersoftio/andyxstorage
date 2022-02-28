using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace Buildersoft.Andy.X.Storage.Core.Provider
{
    public class XNodeConnectionProvider
    {
        private readonly XNodeConfiguration nodeConfig;
        private readonly DataStorageConfiguration dataStorageConfig;
        private readonly AgentConfiguration agentConfiguration;
        private readonly string agentId;

        private HubConnection _connection;

        public XNodeConnectionProvider(XNodeConfiguration nodeConfig,
            DataStorageConfiguration dataStorageConfig,
            AgentConfiguration agentConfiguration,
            string agentId)
        {
            this.nodeConfig = nodeConfig;
            this.dataStorageConfig = dataStorageConfig;
            this.agentConfiguration = agentConfiguration;
            this.agentId = agentId;
            ConnectToXNode();
        }

        private void ConnectToXNode()
        {
            BuildConnectionWithAgent(nodeConfig);
        }

        private void BuildConnectionWithAgent(XNodeConfiguration nodeConfig)
        {
            string serviceUrl = CheckAndFixServiceUrl(nodeConfig.ServiceUrl);
            string location = $"{serviceUrl}/realtime/v2/storage";
            _connection = new HubConnectionBuilder()
                .AddJsonProtocol(opts =>
                {
                    opts.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .WithUrl($"{serviceUrl}/realtime/v2/storage", option =>
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
                    else
                    {
                        option.HttpMessageHandlerFactory = (message) =>
                        {
                            if (message is HttpClientHandler httpClientHandler)
                            {
                                if (nodeConfig.CertificateFile == "")
                                {
                                    httpClientHandler.ServerCertificateCustomValidationCallback +=
                                        (sender, certificate, chain, sslPolicyErrors) => { return true; };
                                    return message;
                                }

                                httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;
                                httpClientHandler.SslProtocols = SslProtocols.Tls12;
                                var certLocation = Path.Combine(SystemLocations.GetConfigCertificateDirectory(), nodeConfig.CertificateFile);
                                httpClientHandler.ClientCertificates.Add(new X509Certificate2(certLocation, nodeConfig.CertificatePassword));
                            }

                            return message;
                        };
                    }

                    option.Headers["x-andyx-storage-name"] = dataStorageConfig.Name;

                    option.Headers["x-andyx-storage-username"] = nodeConfig.Username;
                    option.Headers["x-andyx-storage-password"] = nodeConfig.Password;

                    option.Headers["x-andyx-storage-status"] = dataStorageConfig.Status.ToString();
                    option.Headers["x-andyx-storage-agent-id"] = agentId;
                    option.Headers["x-andyx-storage-agent-max"] = agentConfiguration.MaxNumber.ToString();
                    option.Headers["x-andyx-storage-agent-min"] = agentConfiguration.MinNumber.ToString();
                    option.Headers["x-andyx-storage-agent-loadbalanced"] = agentConfiguration.LoadBalanced.ToString();
                })
                .WithAutomaticReconnect()
                .Build();
        }

        private string CheckAndFixServiceUrl(string serviceUrl)
        {
            if (serviceUrl.EndsWith("/"))
                serviceUrl = serviceUrl.Remove(serviceUrl.Length - 1);
            if (serviceUrl.StartsWith("http") != true)
                serviceUrl = $"https://{serviceUrl}";

            return serviceUrl;
        }

        public HubConnection GetHubConnection()
        {
            return _connection;
        }
    }
}
