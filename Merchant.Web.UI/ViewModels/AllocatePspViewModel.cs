namespace TES.Merchant.Web.UI.ViewModels
{
    public class AllocatePspViewModel
    {
        public long Id { get; set; }
        public byte StatusId { get; set; }
        public byte PspId { get; set; }
        public string ErrorComment { get; set; }
        public string Email { get; set; }
        public  string WebUrl { get; set; }
        public bool? IsVirtualStore { get; set; }
    }
}