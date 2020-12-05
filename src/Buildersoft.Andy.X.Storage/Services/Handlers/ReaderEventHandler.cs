using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class ReaderEventHandler
    {
        private readonly NodeDataStorageService _service;
        private readonly IReaderService _readerService;

        public ReaderEventHandler(NodeDataStorageService service, IReaderService readerService)
        {
            _service = service;
            _readerService = readerService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.ReaderConnectStored += Service_ReaderStored;
            _service.ReaderDisconnectStored += Service_ReaderDisconnectStored;
        }

        private void Service_ReaderDisconnectStored(Data.Model.Events.Readers.ReaderStoredArgs obj)
        {
            _readerService.StoreDisconnectedReader(obj);
        }

        private void Service_ReaderStored(Data.Model.Events.Readers.ReaderStoredArgs obj)
        {
            _readerService.StoreConnectedReader(obj);
        }
    }
}
