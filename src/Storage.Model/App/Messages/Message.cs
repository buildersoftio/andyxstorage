using System;

namespace Buildersoft.Andy.X.Storage.Model.App.Messages
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
        public DateTime CreatedDate { get; set; }

        public Message()
        {
            CreatedDate = DateTime.Now;
        }
    }
}
