using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class BranchTransactionStatusViewModel
    {
        public int YearMonth { get; set; }
        public long MaxPrice { get; set; }
        public long AveragePrice { get; set; }
    }

    public class BranchTransactionStatusViewModel2
    {
        public string Name { get; set; }
        public long Data { get; set; }
    }

    public class BranchTransactionStatusViewModel3
    {
        public int Name { get; set; }
        public IEnumerable<double> SumOfPriceData { get; set; }
        public IEnumerable<double> TotalCountData { get; set; }
        public IEnumerable<double> AverageOfPriceData { get; set; }
        public IEnumerable<double> AverageCountData { get; set; }
    }

    public class BTSViewModel
    {
        public IEnumerable<BranchTransactionStatusViewModel> BTS { get; set; }
        public IEnumerable<BranchTransactionStatusViewModel2> BTS2 { get; set; }
        public IEnumerable<BranchTransactionStatusViewModel3> BTS3 { get; set; }
        public IEnumerable<string> BranchNames { get; set; }
    }
}