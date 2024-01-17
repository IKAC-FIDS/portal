namespace TES.Merchant.Web.UI.ViewModels
{
    public class DeviceTypeViewModel
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public bool IsWireless { get; set; }
        public bool IsActive { get; set; }
        public int BlockPrice { get; set; }
    }
}