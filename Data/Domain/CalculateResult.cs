using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.CalculateResult")]
    public class CalculateResult
    {
        public int Id { get; set; }
        public bool? IsGood { get; set; }
        public double? IsGoodValue { get; set; }
        public int? IsGoodYear { get; set; }
        public int  IsGoodMonth { get; set; }
        public string TerminalNo { get; set; }
        public bool? IsBad { get; set; }
        public bool LowTransaction { get; set; }
        public bool IsActive { get; set; }
        
        public double p_hazineh_soodePardakty  { get; set; }
        public int p_hazineh_rent  { get; set; }
        public double p_hazineh_karmozdShapark  { get; set; }
        public double p_hazineh_hashiyeSood  { get; set; }

        public double p_daramad_Vadie  { get; set; }
        public double p_daramad_Moadel  { get; set; }
        public  double p_daramd_Tashilat { get; set; }
        public double? TransactionValue { get; set; }
        public int? TransactionCount { get; set; }
        public bool? IsInNetwork { get; set; }
        public long? BranchId { get; set; }
        public string AccountNumber { get; set; }

        [NotMapped]
        public string CustomerId
        {
            get
            {
                if (string.IsNullOrEmpty(AccountNumber))
                {
                    return "";
                }

                return AccountNumber.Substring(7, 8);
            }
            
        }

        public string PspTitle { get; set; }
        public string PspId { get; set; }
    }
}