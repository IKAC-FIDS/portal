using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("DamageRequestReply")]
    public class DamageRequestReply
    {
        public long Id { get; set; }
        public long DamageRequestId { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public long UserId { get; set; }

    
        public virtual User User { get; set; }
        public virtual DamageRequest DamageRequest { get; set; }
        public virtual ICollection<DamageRequestReplyDocument> MessageReplyDocuments { get; set; } = new HashSet<DamageRequestReplyDocument>();
    }
}