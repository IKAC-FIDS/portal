using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.TerminalPm")]
    public class TerminalPm
    {
        public int Id { get; set; }

        [Required]
        public DateTime PmTime { get; set; }

        [Required]
        [StringLength(50)]
        public string TerminalNo { get; set; }
    }
}