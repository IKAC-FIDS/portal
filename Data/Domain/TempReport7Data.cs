using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.TempReport7Data")]
    public class TempReport7Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public bool IsWireless { get; set; }
        public bool IsPm { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }
    }
    [Table("dbo.TempReport8Data")]
    public class TempReport8Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public bool IsWireless { get; set; }

        public bool IsGood { get; set; }

        public double GoodValue { get; set; }
        public  string CurrentMonth { get; set; }

        public  string FirstRequest { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }
        public int? InstallationDelay { get; set; }
    }
}