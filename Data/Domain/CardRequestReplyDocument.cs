using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardRequestReplyDocument")]
    public class CardRequestReplyDocument
    {
        public long Id { get; set; }
        public long MessageReplyId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual CardRequestReply CardRequestReply { get; set; }
    }
}