using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.TerminalStatus")]
    public class TerminalStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public byte Id { get; set; }

        [Required] [StringLength(200)] public string Title { get; set; }

        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();

        public virtual ICollection<TempReport1And2Data> TempReport1And2Datas { get; set; } =
            new HashSet<TempReport1And2Data>();
    }
}