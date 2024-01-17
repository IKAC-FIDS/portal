using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class  GetBranchListResponse : BaseResponse
    {
        public int TotalRows { get; set; }
        public List<GetBranchListResponseData> Data { get; set; }
    }
}