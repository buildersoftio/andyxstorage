using Buildersoft.Andy.X.Storage.Data.Model.Books;
using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Events.Books
{
    public class BookCreatedArgs
    {
        public Guid BookId { get; set; }
        public string TenantName { get; set; }
        public string ProductName { get; set; }
        public string ComponentName { get; set; }
        public string BookName { get; set; }
        public DataTypes DataType { get; set; }
    }
}
