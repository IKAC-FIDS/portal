using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("DamageRequestDocument")]
    public class DamageRequestDocument
    {
        public long Id { get; set; }
        public long DamageRequestId { get; set; }
        public byte[] FileData { get; set; }
        public string FileName { get; set; }

        public virtual DamageRequest DamageRequest { get; set; }
    }
}