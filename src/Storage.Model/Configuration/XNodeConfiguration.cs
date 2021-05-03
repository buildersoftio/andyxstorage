﻿namespace Buildersoft.Andy.X.Storage.Model.Configuration
{
    public class XNodeConfiguration
    {
        public string ServiceUrl { get; set; }
        public Subscription Subscription { get; set; }
        public string JwtToken { get; set; }
    }

    public enum Subscription
    {
        Exclusive = 1,
        Shared = 2,
        Backup = 3
    }
}