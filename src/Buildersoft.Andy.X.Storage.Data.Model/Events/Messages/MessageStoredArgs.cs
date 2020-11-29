﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Messages
{
    public class MessageStoredArgs
    {
        public string MessageId { get; set; }
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Book { get; set; }

        public object Message { get; set; }
    }
}
