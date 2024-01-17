using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.RevokeReason")]
    public class RevokeReason
    {
        public int  Order { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public byte Level { get; set; }
        public byte? ParentId { get; set; }
        public virtual RevokeReason Parent { get; set; }
        
    }
}