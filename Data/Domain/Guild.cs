using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.Guild")]
    public class Guild
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
 
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public long? ParentId { get; set; }
        public bool IsActive { get; set; }
        public virtual Guild Parent { get; set; }

        public virtual ICollection<Guild> Children { get; set; } = new HashSet<Guild>();
        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
    }
}