using Buildersoft.Andy.X.Storage.Data.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Readers
{
    public class ReaderStoredArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Book { get; set; }

        public string ReaderName { get; set; }
        public ReaderTypes ReaderType { get; set; }
        public ReaderAs ReaderAs { get; set; }
    }
}
