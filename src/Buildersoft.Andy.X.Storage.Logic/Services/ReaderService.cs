using Buildersoft.Andy.X.Storage.Data.Model.Events.Readers;
using Buildersoft.Andy.X.Storage.FileConfig.Storage.Tenants;
using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Logic.Services
{
    public class ReaderService : IReaderService
    {
        public void StoreReader(ReaderStoredArgs readerStoredArgs)
        {
            string bookLocation = TenantConfigFile.CreateBookLocation(readerStoredArgs.Tenant, readerStoredArgs.Product, readerStoredArgs.Component, readerStoredArgs.Book);
            ReaderConfigFile.SaveReaderConfigFile(bookLocation, readerStoredArgs.ReaderName, readerStoredArgs);
        }
    }
}
