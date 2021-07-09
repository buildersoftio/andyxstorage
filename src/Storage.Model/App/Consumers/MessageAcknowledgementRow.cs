using System;
using System.Text.Json.Serialization;

namespace Buildersoft.Andy.X.Storage.Model.App.Consumers
{
    public class MessageAcknowledgementRow
    {
        [JsonIgnore]
        public string Tenant { get; set; }
        [JsonIgnore]
        public string Product { get; set; }
        [JsonIgnore]
        public string Component { get; set; }
        [JsonIgnore]
        public string Topic { get; set; }
        [JsonIgnore]
        public string Consumer { get; set; }

        public Guid MessageId { get; set; }
        public bool IsAcknowledged { get; set; }
        public DateTime AcknowledgementDate { get; set; }
    }
}
