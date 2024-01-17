namespace TES.Merchant.Web.UI.Service.Models
{
    public class SendChangeAccountRequestResponseModel
    {
        public bool IsSuccess { get; set; }
        public byte StatusId { get; set; }
        public string Result { get; set; }
        public long? RequestId { get; set; }
        public string Error { get; set; }
        public string TopiarId { get; set; }
    }
}