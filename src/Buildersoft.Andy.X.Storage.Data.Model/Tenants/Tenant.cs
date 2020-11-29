using Buildersoft.Andy.X.Storage.Data.Model.Products;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Tenants
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public ConcurrentDictionary<string, Product> Products { get; set; }

        public Encryption Encryption { get; set; }
        public Signature Signature { get; set; }

        public string Location { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Tenant()
        {
            Products = new ConcurrentDictionary<string, Product>();
            Status = true;
        }

    }
    public class Signature
    {
        public string DigitalSignature { get; set; }
        public string SecurityKey { get; set; }
    }
    public class Encryption
    {
        public bool EncryptionStatus { get; set; }
        public string EncryptionKey { get; set; }
    }
}
