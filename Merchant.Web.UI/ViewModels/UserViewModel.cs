namespace TES.Merchant.Web.UI.ViewModels
{
    public class UserViewModel
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string NewPassword { get; set; }
        public string UserName { get; set; }
        public long? BranchId { get; set; }
        public  string PhoneNumber { get; set; }
    }
}