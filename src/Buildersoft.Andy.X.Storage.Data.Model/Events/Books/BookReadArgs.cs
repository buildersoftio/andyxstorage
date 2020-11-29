﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Books
{
    public class BookReadArgs
    {
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }
        public string BookName { get; set; }

        // TODO... Add properties for this Book
    }
}
