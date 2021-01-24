using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Books
{
    public class Schema
    {
        public string Name { get; set; }

        // Check if the schema should be validated before messages are transmitting via Andy.
        public bool SchemaValidationStatus { get; set; }

        public string SchemaRawData { get; set; }
        public int Version { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Schema()
        {
            SchemaValidationStatus = false;
            Version = 1;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }
    }
}
