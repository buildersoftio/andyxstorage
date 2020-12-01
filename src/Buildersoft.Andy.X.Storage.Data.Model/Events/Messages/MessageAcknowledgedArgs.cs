using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Messages
{
    public class MessageAcknowledgedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Book { get; set; }
        public string Reader { get; set; }
        public string MessageId { get; set; }
    }
}
