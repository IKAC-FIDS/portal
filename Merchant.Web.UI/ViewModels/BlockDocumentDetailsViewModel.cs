using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class BlockDocumentDetailsViewModel
    {
        public long? TerminalId { get; set; }
        public string TerminalNo { get; set; }
        public DateTime? BlockDocumentDate { get; set; }
        public string BlockDocumentNumber { get; set; }
        public string BlockAccountNumber { get; set; }
        public int? BlockPrice { get; set; }
        public byte? PreferredPspId { get; set; }
        public byte? BlockDocumentStatusId { get; set; }
        public long? BlockDocumentId { get; set; }
    }
}