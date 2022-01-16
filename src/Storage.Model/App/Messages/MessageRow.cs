using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Model.App.Messages
{
    public class MessageRow
    {
        public Guid Id { get; set; }
        public object MessageRaw { get; set; }
        public DateTime StoredDate { get; set; }
        public MessageRow()
        {
            StoredDate = DateTime.Now;
        }
    }
}
