using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("CardRequestDocument")]
    public class CardRequestDocument
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual CardRequest CardRequest { get; set; }
    }
}