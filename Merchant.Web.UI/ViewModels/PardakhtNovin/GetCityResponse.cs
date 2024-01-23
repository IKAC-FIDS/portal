using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class GetCityResponse
    {
        public int TotalRows { get; set; }
        public List<GetCityResponseData> Data { get; set; }
    }
}