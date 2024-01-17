using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace TES.Data.Domain
{
    [Table("psp.Psp")]
    public class Psp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public virtual ICollection<PspAgent> PspAgents { get; set; } = new HashSet<PspAgent>();
        public virtual ICollection<Terminal> PreferredByTerminals { get; set; } = new HashSet<Terminal>();
        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
        
        public  virtual  ICollection<PspBranchRate> PspBranchRate { get; set; }
    }
}