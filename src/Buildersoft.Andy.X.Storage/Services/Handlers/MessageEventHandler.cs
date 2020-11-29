using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class MessageEventHandler
    {
        private readonly SignalRDataStorageService _service;
        private readonly IMessageService _messageService;

        public MessageEventHandler(SignalRDataStorageService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.MessageStored += DataStorageService_MessageStored;
        }

        private void DataStorageService_MessageStored(Data.Model.Events.Messages.MessageStoredArgs obj)
        {
            _messageService.StoreMessage(obj);
        }
    }
}
