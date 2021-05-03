using Buildersoft.Andy.X.Storage.Logic.Services.Interfaces;
using Microsoft.Extensions.Logging;

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
            _service.MessageLogStored += DataStorageService_MessageLogStored;
        }

        private void DataStorageService_MessageLogStored(Data.Model.Events.Messages.MessageLogedArgs obj)
        {
            _messageService.StoreMessageLogToReader(obj);
        }

        private void DataStorageService_MessageStored(Data.Model.Events.Messages.MessageStoredArgs obj)
        {
            _messageService.StoreMessage(obj);
        }
    }
}
