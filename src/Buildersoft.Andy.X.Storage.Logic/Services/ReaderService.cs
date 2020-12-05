using Buildersoft.Andy.X.Storage.Data.Model.Events.Readers;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class ReaderService : IReaderService
    {
        private readonly ILogger<ReaderService> _logger;

        public ReaderService(ILogger<ReaderService> logger)
        {
            _logger = logger;
        }
        public void StoreConnectedReader(ReaderStoredArgs readerStoredArgs)
        {
            string bookLocation = TenantConfigFile.CreateBookLocation(readerStoredArgs.Tenant, readerStoredArgs.Product, readerStoredArgs.Component, readerStoredArgs.Book);
            if (ReaderConfigFile.SaveReaderConfigFile(bookLocation, readerStoredArgs.ReaderName, readerStoredArgs) != true)
                _logger.LogError($"{readerStoredArgs.Tenant}/{readerStoredArgs.Product}/{readerStoredArgs.Component}/{readerStoredArgs.Book}/readers/{readerStoredArgs.ReaderName}: failed");

            _logger.LogInformation($"{readerStoredArgs.Tenant}/{readerStoredArgs.Product}/{readerStoredArgs.Component}/{readerStoredArgs.Book}/readers/{readerStoredArgs.ReaderName}: connected");
        }

        public void StoreDisconnectedReader(ReaderStoredArgs readerStoredArgs)
        {
            string bookLocation = TenantConfigFile.CreateBookLocation(readerStoredArgs.Tenant, readerStoredArgs.Product, readerStoredArgs.Component, readerStoredArgs.Book);
            string logLine = $"{DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss")}\t{readerStoredArgs.ReaderName}\tdisconnected";
            ReaderConfigFile.StoreLogInReader(bookLocation, readerStoredArgs.ReaderName, logLine);
        }
    }
}
