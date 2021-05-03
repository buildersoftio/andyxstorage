using Buildersoft.Andy.X.Storage.Model.Shared.Products;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Model.Shared.Tenants
{
    public class Tenant
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
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
