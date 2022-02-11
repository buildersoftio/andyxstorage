using Buildersoft.Andy.X.Storage.Model.App.Components;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Components
{
    public class ComponentTokenCreatedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }

        public ComponentToken Token { get; set; }
        public List<string> StoragesAlreadySent { get; set; }


        public ComponentTokenCreatedArgs()
        {
            Token = new ComponentToken();
            StoragesAlreadySent = new List<string>();
        }
    }
}
