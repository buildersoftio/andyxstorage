namespace Buildersoft.Andy.X.Storage.Model.Events.Consumers
{
    public class ConsumerDisconnectedArgs
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }
        public string ConsumerName { get; set; }
    }
}
