using System;
using TES.Common.Extensions;

namespace TES.Data.DataModel
{
    public class InstallationDelayData
    {
        public string TerminalNo { get; set; }
        public int DelayCount { get; set; }
        public int DelayCountWithoutHoliday { get; set; }
        public bool HasDelay { get; set; }
        public long BranchId { get; set; }
        public byte StatusId { get; set; }
        public bool IsWireless { get; set; }
        public byte PspId { get; set; }
        public string PspTitle { get; set; }
        public string StatusTitle { get; set; }
        public string BranchTitle { get; set; }
        public string DeviceTypeTitle { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime BatchDate { get; set; }
        public string PersianBatchDate => BatchDate.ToPersianDate();
        public string PersianInstallationDate => InstallationDate.ToPersianDate();
    }
}