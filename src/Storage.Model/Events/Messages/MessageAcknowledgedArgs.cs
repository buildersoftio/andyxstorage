namespace Buildersoft.Andy.X.Storage.Model.Events.Messages
{
    public class MessageAcknowledgedArgs
    {
        public string MessageId { get; set; }
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }

        // By which consumer has been acknowledged.
        public string ConsumerName { get; set; }
    }
}
