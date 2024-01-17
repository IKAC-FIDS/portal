using System;
using System.ComponentModel.DataAnnotations.Schema;
using TES.Common.Enumerations;

namespace TES.Data.Domain
{
    [Table("dbo.TempReport1And2Data")]
    public class TempReport1And2Data
    {
        public long Id { get; set; }
        public string TerminalNo { get; set; }
        public bool IsWireless { get; set; }
        public byte? StatusId { get; set; }
        public DateTime SubmitTime { get; set; }
        public DateTime? BatchDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? RevokeDate { get; set; }
        public byte Month { get; set; }
        public short Year { get; set; }

      //  public  string Statuses { get; set; }
        public virtual TerminalStatus Status { get; set; }
    }
}