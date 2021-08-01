using Buildersoft.Andy.X.Storage.Model.App.Messages;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers
{
    public class ConsumerMessage
    {
        public Message Message { get; set; }
        public string Consumer { get; set; }
        public ConsumerMessage()
        {
            Message = new Message();
        }
    }
}
