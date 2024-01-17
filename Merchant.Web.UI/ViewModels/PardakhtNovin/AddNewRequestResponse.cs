namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewRequestResponse : BaseResponse
    {
        public  int? SavedID { get; set; }
        public AddNewRequestReposneData Data { get; set; }
    }

    public class AddNewRequestReposneData
    {
        public  string FollowupCode { get; set; }
    }
}