using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("MessageStatus")]
    public class MessageStatus
    {
        public byte Id { get; set; }
        public string Title { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }
}