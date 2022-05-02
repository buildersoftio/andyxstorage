using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buildersoft.Andy.X.Storage.Model.Entities
{
    public class Message
    {
        [Key]
        public Guid MessageId { get; set; }

        [Column(TypeName = "json")]
        public string Payload { get; set; }

        [Column(TypeName = "json")]
        public string Headers { get; set; }

        public DateTime StoredDate { get; set; }
        public DateTime SentDate { get; set; }
    }
}
