using System;
using TES.Common.Extensions;

namespace TES.Data.DataModel
{
    public class ChangeAccountRequestData
    {
        public long ChangeAccountRequestId { get; set; }
        public long TerminalId { get; set; }
        public string TerminalNo { get; set; }
        public string ContractNo { get; set; }
        public string RequestStatus { get; set; }
        public DateTime SubmitTime { get; set; }
        public string ShebaNumber { get; set; }
        public string RequestedAccountNo { get; set; }
        public string OldAccountNo { get; set; }
        public string CurrentTerminalAccountNo { get; set; }
        public string Result { get; set; }
        public string SubmitterUserFullName { get; set; }
        public string TerminalStatus { get; set; }
        public string BranchTitle { get; set; }
        public byte PspId { get; set; }
        public string PspTitle { get; set; }
        public byte StatusId { get; set; }
        public long? RequestId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public string PersianSubmitTime => SubmitTime.ToPersianDateTime();
        public string RelativeSubmitTime => SubmitTime.ToRelativeDate();
        public bool IsOldFanava => !string.IsNullOrEmpty(ContractNo) && ContractNo.StartsWith("FS");

        public string Error { get; set; }
        public bool IsDone => CurrentTerminalAccountNo == RequestedAccountNo;
    }
}