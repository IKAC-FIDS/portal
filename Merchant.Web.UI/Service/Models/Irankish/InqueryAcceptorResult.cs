using System;

namespace TES.Merchant.Web.UI.Service.Models.Irankish
{
    public class InqueryAcceptorResult
    {
        public bool IsSuccess { get; set; }
        public byte StatusId { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime? RevokeDate { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string ShebaNo { get; set; }
        public string ErrorComment { get; set; }
        public string Description { get; set; }
        public string AccountNo { get; set; }
        public string TerminalNo { get; set; }
        public DateTime BatchDate { get; set; }
    }
}