using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Services.Handlers
{
    public class MessageEventHandler
    {
        private readonly ILogger<MessageEventHandler> _logger;
        private readonly NodeDataStorageService _service;
        private readonly IMessageService _messageService;

        public MessageEventHandler(ILogger<MessageEventHandler> logger, NodeDataStorageService service, IMessageService messageService)
        {
            _service = service;
            _messageService = messageService;
            _logger = logger;

            InitializeEvents();
        }

        private void InitializeEvents()
        {
            _service.MessageStored += DataStorageService_MessageStored;
            _service.MessageAcknowledgeStored += DataStorageService_MessageAcknowledgeStored;
        }

        private void DataStorageService_MessageAcknowledgeStored(Data.Model.Events.Messages.MessageAcknowledgedArgs obj)
        {
            //TODO... Implemented and store the acked message from the reader.
        }

        private void DataStorageService_MessageStored(Data.Model.Events.Messages.MessageStoredArgs obj)
        {
            _messageService.StoreMessage(obj);
        }
    }
}
