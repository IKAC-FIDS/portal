using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class GetBankResponse : BaseResponse
    {
        public int TotalRows { get; set; }
        public List<GetBackResponseData> Data { get; set; }
    }
}