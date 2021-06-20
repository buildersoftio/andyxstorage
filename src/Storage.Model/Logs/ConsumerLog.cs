namespace Buildersoft.Andy.X.Storage.Model.Logs
{
    public class ConsumerLog
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }
        public string ConsumerName { get; set; }
        public string Log { get; set; }
    }
}
