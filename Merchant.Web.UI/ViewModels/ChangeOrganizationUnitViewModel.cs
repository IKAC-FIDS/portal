namespace TES.Merchant.Web.UI.ViewModels
{
    public class ChangeOrganizationUnitViewModel
    {
        public long UserId { get; set; }
        public long? BranchId { get; set; }
    }
    
    public class ChangePhoneNumberViewModel
    {
        public long UserId { get; set; }
        public string PhoneNumber { get; set; }
    }
}