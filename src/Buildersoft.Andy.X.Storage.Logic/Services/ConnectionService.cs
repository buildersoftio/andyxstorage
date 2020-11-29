using Buildersoft.Andy.X.Storage.Data.Model;
using Buildersoft.Andy.X.Storage.Data.Model.Tenants;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Repositories.Interfaces;
using Buildersoft.Andy.X.Storage.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class ConnectionService
    {
        private HttpClient client;
        private readonly ITenantRepository _tenantRepository;
        private readonly HttpClientHandler _httpClientHandler;

        public ConnectionService(ITenantRepository tenantRepository, HttpClientHandler httpClientHandler)
        {
            _tenantRepository = tenantRepository;
            _httpClientHandler = httpClientHandler;
        }

        public bool ConnectToAndyX(AndyXProperty andyXProperty)
        {
            client = new HttpClient(_httpClientHandler);
            if (andyXProperty.Token != "")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", andyXProperty.Token);

            string requestUrl = $"{andyXProperty.Url}/datastorages/ping";

            HttpResponseMessage response = client.GetAsync(requestUrl).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            return true;
        }

        public async Task<bool> RegisterToAndyXAsync(DataStorage dataStorage, AndyXProperty andyX)
        {
            client = new HttpClient(_httpClientHandler);
            if (andyX.Token != "")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", andyX.Token);

            string requestUrl = $"{andyX.Url}/datastorages/ping";

            HttpResponseMessage response = client.GetAsync(requestUrl).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

            requestUrl = $"{andyX.Url}/datastorages/add";
            var bodyRequestContent = new StringContent(dataStorage.ToJson(), UnicodeEncoding.UTF8, "application/json");

            response = client.PostAsync(requestUrl, bodyRequestContent).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
                return false;

            if (await GetAndyXCurrentStateAsync(andyX) != true)
                return false;

            return true;
        }

        public async Task<bool> GetAndyXCurrentStateAsync(AndyXProperty andyX)
        {
            client = new HttpClient(_httpClientHandler);
            if (andyX.Token != "")
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", andyX.Token);

            string requestUrl = $"{andyX.Url}/datastorages/ping";

            HttpResponseMessage response = client.GetAsync(requestUrl).Result;
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return false;


            requestUrl = $"{andyX.Url}/datastorages/currentstate";

            response = client.GetAsync(requestUrl).Result;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string result = await response.Content.ReadAsStringAsync();
                var tenants = JsonConvert.DeserializeObject<ConcurrentDictionary<string, Tenant>>(result);

                // Get here and update the tenant data.
                foreach (var tenant in tenants)
                {
                    tenant.Value.Location = TenantConfigFile.CreateTenantLocation(tenant.Key);
                    foreach (var product in tenant.Value.Products)
                    {
                        product.Value.Location = TenantConfigFile.CreateProductLocation(tenant.Key, product.Key);
                        foreach (var component in product.Value.Components)
                        {
                            component.Value.Location = TenantConfigFile.CreateComponentLocation(tenant.Key, product.Key, component.Key);
                            foreach (var book in component.Value.Books)
                            {
                                book.Value.Location = TenantConfigFile.CreateBookLocation(tenant.Key, product.Key, component.Key, book.Key);
                                book.Value.Messages = new ConcurrentDictionary<string, Data.Model.Messages.Message>();
                                book.Value.Readers = new ConcurrentDictionary<string, Data.Model.Readers.Reader>();
                            }
                        }
                    }
                }
                return _tenantRepository.Update(tenants);
            }
            return false;
        }
    }
}
