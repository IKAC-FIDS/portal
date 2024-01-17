namespace TES.Merchant.Web.UI.ViewModels
{
    public class GetTerminalEmDataViewModel
    {
        public byte? PspId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public bool RetriveTotalPageCount { get; set; }
        public int Page { get; set; }
    }
}