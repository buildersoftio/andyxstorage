using System;
using System.ComponentModel.DataAnnotations;

namespace Buildersoft.Andy.X.Storage.Model.Entities
{
    public class ConsumerMessage
    {
        [Key]
        public Guid MessageId { get; set; }

        public string PartitionFile { get; set; }
        public int PartitionIndex { get; set; }

        public bool IsAcknowledged { get; set; }

        public DateTime? SentDate { get; set; }
        public DateTime? AcknowledgedDate { get; set; }
    }
}
