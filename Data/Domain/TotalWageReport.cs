using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.TotalWageReport")]
    public class TotalWageReport
    {
        public  int Id { get; set; }
        public  int Year { get; set; }
        public  int Month { get; set; }
        public  double Value { get; set; }
        public  int TerminalCount { get; set; }
        public double OtherValue { get; set; }
        public int OtherTerminalCount { get; set; }
        public double PmValue { get; set; }
        public double OtherPmValue { get; set; }
        public int PmTerminalCount { get; set; }
        public int OtherPmTerminalCount { get; set; }
    }
}