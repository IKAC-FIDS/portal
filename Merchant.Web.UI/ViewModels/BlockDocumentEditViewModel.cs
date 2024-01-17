using System;
using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class BlockDocumentEditViewModel
    {
        public long TerminalId { get; set; }
        public string TerminalNo { get; set; }
        public string BlockAccountNumber { get; set; }
        public DateTime? BlockDocumentDate { get; set; }
        public string BlockDocumentNumber { get; set; }
        public int? BlockPrice { get; set; }
        public byte? PreferredPspId { get; set; }
        public byte? BlockDocumentStatusId { get; set; }
        public long? BlockDocumentId { get; set; }
        public string AccountRow { get; set; }
        public string AccountCustomerNumber { get; set; }
        public string AccountType { get; set; }
        public string AccountBranchCode { get; set; }
        public HttpPostedFileBase Document { get; set; }
    }
}