using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;

namespace TES.Data.Domain
{
    [Table("dbo.BranchPermission")]
    public class BranchPermission
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  Id { get; set; }

        public string Title { get; set; }
        public long BranchId { get; set; }
        public string Status { get; set; }
        
        public int TotalTerminalCount { get; set; }
        public int TotalLowTransactionTerminalCount { get; set; }
        public string LowtoAllPercent { get; set; }
        public int WifiDeviceCount { get; set; }
        public string WifitoallDevicePercentage { get; set; }
        public int LowCount { get; set; }
        public int StatusId { get; set; }
        public string RoundLowtoAllPercent { get; set; }
        [NotMapped]
        public int Month { get; set; }
    }
}