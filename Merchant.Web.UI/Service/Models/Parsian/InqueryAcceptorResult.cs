using System;

namespace TES.Merchant.Web.UI.Service.Models.Parsian
{
    public class InqueryAcceptorResult
    {
        public bool IsSuccess { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string Description { get; set; }
        public string MerchantNo { get; set; }
        public string TerminalNo { get; set; }
        public DateTime? InstallationDate { get; set; }
    }
}