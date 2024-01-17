using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.CustomerStatusResult")]
    public class CustomerStatusResult
    {
        public long Id { get; set; }
        public bool? IsGood { get; set; }
        public int? IsGoodYear { get; set; }
        public int? IsGoodMonth { get; set; }
        public string CustomerId { get; set; }
        public double Daramad { get; set; }
        public double Hazineh { get; set; }
        public double IsGoodValue { get; set; }
        public double Avg { get; set; }
        public bool? Special { get; set; }
        public  long? BranchId { get; set; }
        public virtual OrganizationUnit Branch { get; set; }
        public int? TransactionCount { get; set; }
        public double? TransactionValue { get; set; }
    }
}