namespace TES.Merchant.Web.UI.ViewModels
{
    public class RevokeRequestViewModel
    {
        public long? Id { get; set; }
        public string TerminalNo { get; set; }
        public byte ReasonId { get; set; }
        public byte? SecondReasonId { get; set; }
        public string ReasonDescription { get; set; }
        public string DeliveryDescription { get; set; }
    }
}