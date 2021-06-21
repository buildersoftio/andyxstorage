﻿using System;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Messages
{
    public class MessageStoredArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }
        public List<string> ConsumersCurrentTransmitted { get; set; }

        public Guid Id { get; set; }
        public object MessageRaw { get; set; }
    }
}
