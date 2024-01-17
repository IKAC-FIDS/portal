using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class GetUserActivityLogDataViewModel
    {
        public long UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool RetriveTotalPageCount { get; set; }
        public int Page { get; set; }
    }
}