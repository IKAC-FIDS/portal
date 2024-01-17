using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class BlockDocumentSearchViewModel
    {
        public long? TerminalId { get; set; }
        public string TerminalNo { get; set; }
        public List<byte> TerminalStatusIdList { get; set; }
        public byte? BlockDocumentStatusId { get; set; }
        public string FullName { get; set; }
        public byte? PspId { get; set; }
        public long? DeviceTypeId { get; set; }
        public string NationalCode { get; set; }
        public string Title { get; set; }
        public long? BranchId { get; set; }
        public string CustomerNumber { get; set; }
        public long? MarketerId { get; set; }
        public DateTime? FromBlockDocumentDate { get; set; }
        public DateTime? ToBlockDocumentDate { get; set; }
        public string BranchCode { get; set; }
        public  int? StatusId { get; set; }
    }
}