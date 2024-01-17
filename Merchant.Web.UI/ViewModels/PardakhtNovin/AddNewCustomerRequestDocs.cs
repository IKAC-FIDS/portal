using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewCustomerRequestDocs
    {
        public string ChildName { get; set; } = "CustomerDocument";
        public List<Document> Data { get; set; }
    }

    public class  InqueryAcceptorResult
    {
        public bool IsSuccess { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public string AccountNo { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string Description { get; set; }
        public string ErrorComment { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime? InstallationDate { get; set; }
        public int Status { get; set; }
        public DateTime? DisMountDate { get; set; }
        public DateTime? MountDate { get; set; }
        public string Terminal { get; set; }
        public string Acceptor { get; set; }
        public string ShaparakResponseFa { get; set; }
        public string TerminalNo { get; set; }
    }
}