﻿namespace Buildersoft.Andy.X.Storage.Model.Configuration
{
    public class PartitionConfiguration
    {
        public int SizeInMemory { get; set; }
        public int BatchSize { get; set; }
        public int FlushInterval { get; set; } // its in miliseconds
        public double PointerAcknowledgedMessageArchivationInterval { get; set; } // its in miliseconds
    }
}
