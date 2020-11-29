using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Messages
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Data { get; set; }
        public byte[] DataAsBytes { get; set; }
        public DateTime CreatedDate { get; set; }

        public Message()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
        }
        public void SetId(Guid id)
        {
            Id = id;
        }
    }
}
