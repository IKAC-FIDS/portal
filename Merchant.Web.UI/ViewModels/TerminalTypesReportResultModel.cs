using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class TerminalTypesReportResultModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public long WirelessCount { get; set; }
        public long WithWireCount { get; set; }

        public long TotalCount => WirelessCount + WithWireCount;
        public decimal WithWirePercentage => Math.Round((decimal)WithWireCount / TotalCount * 100);
        public decimal WirelessPercentage => Math.Round((decimal)WirelessCount / TotalCount * 100);
        public bool AllowWireless => WirelessPercentage <= 30;
    }
}