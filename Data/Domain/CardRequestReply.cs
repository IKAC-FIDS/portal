using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardRequestReply")]
    public class CardRequestReply
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public long UserId { get; set; }

    
        public virtual User User { get; set; }
        public virtual CardRequest CardRequest { get; set; }
        public virtual ICollection<CardRequestReplyDocument> MessageReplyDocuments { get; set; } = new HashSet<CardRequestReplyDocument>();
    }
}