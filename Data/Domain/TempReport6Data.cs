using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("dbo.TempReport6Data")]
    public class TempReport6Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public string Subject { get; set; }
        public DateTime FirstOperationDate { get; set; }
        public DateTime LastOperationDate { get; set; }
        public short InstallationDelay { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }
        public string Type { get; set; }
        public string City { get; set; }
        public string Sla { get; set; }
        public string ValidDay { get; set; }
    }
}