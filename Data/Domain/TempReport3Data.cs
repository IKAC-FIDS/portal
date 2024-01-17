using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.TempReport3Data")]
    public class TempReport3Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public long BranchCode { get; set; }
        public string BranchTitle { get; set; }
        public short Year { get; set; }
        public byte Month { get; set; }
    }
}