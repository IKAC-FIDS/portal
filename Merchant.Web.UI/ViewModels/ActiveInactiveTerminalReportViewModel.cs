namespace TES.Merchant.Web.UI.ViewModels
{
    public class ActiveInactiveTerminalReportViewModel
    {
        public long MarketerId { get; set; }
        public string MarketerTitle { get; set; }
        public string YearMonth { get; set; }
        public long WithSimActiveTerminalCount { get; set; }
        public long WithSimInactiveTerminalCount { get; set; }
        public long WithoutSimActiveTerminalCount { get; set; }
        public long WithoutSimInactiveTerminalCount { get; set; }
    }
}