using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("DamageRequestReplyDocument")]
    public class DamageRequestReplyDocument
    {
        public long Id { get; set; }
        public long DamageRequestReplyId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual DamageRequestReply DamageRequestReply { get; set; }
    }
}