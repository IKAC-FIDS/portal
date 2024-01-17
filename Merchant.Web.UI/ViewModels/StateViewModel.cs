using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class StateViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public List<CityViewModel> Cities { get; set; }
    }
}