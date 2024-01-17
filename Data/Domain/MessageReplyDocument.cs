using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("MessageReplyDocument")]
    public class MessageReplyDocument
    {
        public long Id { get; set; }
        public long MessageReplyId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual MessageReply MessageReply { get; set; }
    }
}