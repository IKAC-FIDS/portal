using System;
using TES.Common.Extensions;

namespace TES.Data.DataModel
{
    public class TerminalEmData
    {
        public byte StatusId { get; set; }
        public string StatusTitle { get; set; }
        public string TerminalNo { get; set; }
        public string PspTitle { get; set; }
        public byte? PspId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public bool IsWireless { get; set; }
        public DateTime RequestEmTime { get; set; }
        public DateTime? EmTime { get; set; }
        public int HolidayCount { get; set; }
        public string PersianRequestEmTime => RequestEmTime.ToPersianDate();
        public string PersianEmTime => EmTime.HasValue ? EmTime.ToPersianDate() : string.Empty;
        public int? Difference => EmTime.HasValue ? Math.Max((EmTime.Value - RequestEmTime).Days - 1 - HolidayCount, 0) : (int?)null;
    }
}