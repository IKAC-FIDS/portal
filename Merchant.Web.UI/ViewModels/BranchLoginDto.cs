namespace TES.Merchant.Web.UI.ViewModels
{
    public class BranchLoginDto
    {
        public  string UserName { get; set; }
        public  string Password { get; set; }
    }

    public class KhafanResultDto
    {
        public  string TerminalNo { get; set; }
        public  string MerchantTitle { get; set; }
        public  int  NoTransactionMount { get; set; }
        public string MerchantNationalCode { get; set; }
        public int LowTransactionMount { get; set; }
        
        public  int HighTransactionMount { get; set; }
        
        public  double LowestTransaction { get; set; }
        public  double HighestTransaction { get; set; }
        public long Average { get; set; }
        public string Senf { get; set; }
        public string Status { get; set; }
        public string TerminalTitle { get; set; }
    }
}