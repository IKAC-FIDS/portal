using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class GetGuildsResponse : BaseResponse
    {
        public int TotalRows { get; set; }
        public List<GetGuildsResponseData> Data { get; set; }
    }
}