using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model.Books
{
    public class Fragmentation
    {
        public string CurrentFragmentId { get; set; }
        public int MaxNumberOfRecordsForFragment { get; set; }

        public Fragmentation()
        {
            CurrentFragmentId = "000000";
            MaxNumberOfRecordsForFragment = 10000;
        }
    }
}
