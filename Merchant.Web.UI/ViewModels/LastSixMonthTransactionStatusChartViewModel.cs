namespace TES.Merchant.Web.UI.ViewModels
{
    public class LastSixMonthTransactionStatusChartViewModel
    {
        public string PersianLocalYearMonth { get; set; }
        public int HighTransactionCount { get; set; }
        public int LowTransactionCount { get; set; }
        public int WithoutTransactionCount { get; set; }
    }
}