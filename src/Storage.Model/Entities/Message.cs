using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Buildersoft.Andy.X.Storage.Model.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }
        public string Payload { get; set; }
        public string Headers { get; set; }
        public DateTime StoredDate { get; set; }
        public DateTime SentDate { get; set; }
    }
}
