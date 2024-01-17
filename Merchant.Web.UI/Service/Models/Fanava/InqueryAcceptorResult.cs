using System;

namespace TES.Merchant.Web.UI.Service.Models.Fanava
{
    public class InqueryAcceptorResult
    {
        public bool IsSuccess { get; set; }
        public long TerminalId { get; set; }
        public string ContractNo { get; set; }
        public byte? TerminalStatus { get; set; }
        public string AccountNo { get; set; }
        public string ShebaNo { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? BatchDate { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string TerminalNo { get; set; }
        public string MerchantNo { get; set; }
        public DateTime? ContractDate { get; set; }
        public string ErrorComment { get; set; }
    }
}