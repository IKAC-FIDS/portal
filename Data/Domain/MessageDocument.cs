using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("MessageDocument")]
    public class MessageDocument
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual Message Message { get; set; }
    }
}