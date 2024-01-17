using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("TerminalNote")]
    public class TerminalNote
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Body { get; set; }

        public long TerminalId { get; set; }
        public long SubmitterUserId { get; set; }
        public DateTime SubmitTime { get; set; }

        public virtual Terminal Terminal { get; set; }
        public virtual User SubmitterUser { get; set; }
    }
}