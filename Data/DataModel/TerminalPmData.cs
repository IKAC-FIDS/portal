using System;

namespace TES.Data.DataModel
{
    public class TerminalPmData
    {
        public byte StatusId { get; set; }
        public string StatusTitle { get; set; }
        public string TerminalNo { get; set; }
        public string PspTitle { get; set; }
        public byte? PspId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public bool IsWireless { get; set; }
        public DateTime? PmTime { get; set; }
    }
}