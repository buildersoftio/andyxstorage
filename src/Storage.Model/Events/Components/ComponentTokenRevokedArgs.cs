using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.Model.Events.Components
{
    public class ComponentTokenRevokedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }

        public string Token { get; set; }
        public List<string> StoragesAlreadySent { get; set; }

        public ComponentTokenRevokedArgs()
        {
            StoragesAlreadySent = new List<string>();
        }
    }
}
