using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TES.Common.Extensions;

namespace TES.Data.Domain
{
    [Table("MessageReply")]
    public class MessageReply
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public long UserId { get; set; }

    
        public virtual User User { get; set; }
        public virtual Message Message { get; set; }
        public virtual ICollection<MessageReplyDocument> MessageReplyDocuments { get; set; } = new HashSet<MessageReplyDocument>();
    }
}