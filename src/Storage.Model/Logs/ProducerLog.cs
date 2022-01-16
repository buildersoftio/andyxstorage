namespace Buildersoft.Andy.X.Storage.Model.Logs
{
    public class ProducerLog
    {
        public string Tenant { get; set; }
        public string Product { get; set; }
        public string Component { get; set; }
        public string Topic { get; set; }
        public string ProducerName { get; set; }
        public string Log { get; set; }
    }
}
