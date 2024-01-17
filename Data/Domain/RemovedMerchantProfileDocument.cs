using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RemovedMerchantProfileDocument")]
    public class RemovedMerchantProfileDocument
    {
        public long Id { get; set; }
        public long RemovedMerchantProfileId { get; set; }

        [Required]
        public byte[] FileData { get; set; }

        public long DocumentTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FileName { get; set; }

        public virtual DocumentType DocumentType { get; set; }
        public virtual RemovedMerchantProfileDocument RemovedMerchantProfile { get; set; }
        
        public string ContentType { get; set; }
    }
}