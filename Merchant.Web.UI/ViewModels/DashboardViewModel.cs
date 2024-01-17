using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            Rating = new List<PeopleRatingViewModel>();
        }
        public int OpenTicketCount { get; set; }
        public int NeedToReformTerminalCount { get; set; }
        public int NewCount { get; set; }
        public int WaitingForRevokeCount { get; set; }

        public object[][] TerminalsByStatusData { get; set; }
        public object[][] TerminalsByPspData { get; set; }
        public object[][] TerminalsByDeviceTypeData { get; set; }
        public ChartViewModel TerminalsByStateChart { get; set; }
        public List<NewsViewModel> News { get; set; }
        public  List<PeopleRatingViewModel> Rating { get; set; }



        public string LastUpdate { get; set; } = "";
        
        public double FaRate { get; set; }
        public double IkRate { get; set; }
        public double PaRate { get; set; }
    }

    public  class RatingViewModel
    {
    public  List<PeopleRatingViewModel> Rating { get; set; }
        
      
         
    public string LastUpdate{get;set;}
    }
    public class TransactionChartsViewModel
    {
        public ChartViewModel TransactionsPriceChart { get; set; }
        public ChartViewModel TransactionsCountChart { get; set; }
    }

    public class ChartViewModel
    {
        public IEnumerable<long> ChartData { get; set; }
        public IEnumerable<string> ChartCategories { get; set; }
    }
}