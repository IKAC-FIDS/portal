using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("MessageSubject")]
    public class MessageSubject
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; } = new HashSet<Message>();
        public int? ParentId { get; set; }
        public int? PspId { get; set; }
    }
}