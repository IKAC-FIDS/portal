using System.ComponentModel.DataAnnotations;
using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class ChangeAccountRequestViewModel
    {
        public long Id { get; set; }
        public string BranchTitle { get; set; } = "";

        [Required]
        public string AccountRow { get; set; }

        [Required]
        public string AccountCustomerNumber { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Required]
        public string AccountBranchCode { get; set; }

        public string TerminalNo { get; set; }

        public byte? PspId { get; set; }

        public HttpPostedFileBase PostedFile { get; set; }

        public string CurrentBranchTitle { get; set; }
        public string CurrentAccountNo { get; set; }
    }
    
    public class  RuleDefinitionRequestViewModel
    {
        public int Id { get; set; }

        

        public int PspId { get; set; }

        public HttpPostedFileBase PostedFile { get; set; }
        public int DeviceTypeId { get; set; }
        public int RuleTypeId { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        public double Rate { get; set; }
        public string BranchTitle { get; set; } = "";
    }


    public class PspRating
    {


      
        public string Psp { get; set; }
  
        public double Rate { get; set; }
        
    }
}