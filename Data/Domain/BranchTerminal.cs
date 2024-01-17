using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.BranchTerminal")]
    public class BranchTerminal
    {
        [Key] public long Id { get; set; }
        public string TerminalNo { get; set; }
        public long BranchCode { get; set; }
        public string BranchTitle { get; set; }
        public DateTime? RevokeDate { get; set; }
        public DateTime? InstallationDate { get; set; }
    }
}