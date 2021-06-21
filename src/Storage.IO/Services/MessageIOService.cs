using Microsoft.Extensions.Logging;

namespace Buildersoft.Andy.X.Storage.IO.Services
{
    public class MessageIOService
    {
        private readonly ILogger<MessageIOService> logger;

        public MessageIOService(ILogger<MessageIOService> logger)
        {
            this.logger = logger;
        }

        // TODO: Implement the logic of files per message.
    }
}
