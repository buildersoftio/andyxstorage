namespace Buildersoft.Andy.X.Storage.Model.Events.Messages
{
    public class MessageStoredArgs
    {
        public string MessageId { get; set; }
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }

        public object Message { get; set; }
    }
}
