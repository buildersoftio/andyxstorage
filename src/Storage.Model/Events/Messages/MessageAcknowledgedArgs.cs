﻿using System;

namespace Buildersoft.Andy.X.Storage.Model.Events.Messages
{
    public class MessageAcknowledgedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }

        public string Consumer { get; set; }
        public bool IsAcknowledged { get; set; }
        public Guid MessageId { get; set; }
    }
}
