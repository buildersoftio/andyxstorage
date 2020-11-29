using System;
using System.Collections.Generic;
using System.Text;

namespace Buildersoft.Andy.X.Storage.Data.Model
{
    public class AndyXProperty
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string Token { get; set; }
        public AndyXProperty()
        {
            Name = "";
        }
    }
}
