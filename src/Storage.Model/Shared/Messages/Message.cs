using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Model.Shared.Messages
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
