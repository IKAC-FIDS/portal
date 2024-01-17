using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.TempReport5Data")]
    public class TempReport5Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public bool IsWireless { get; set; }
        public DateTime SubmitTime { get; set; }
        public DateTime? BatchDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public short InstallationDelay { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }
    }
}