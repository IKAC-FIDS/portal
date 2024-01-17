namespace TES.Merchant.Web.UI.Service.Models
{
    public class UpdateTerminalStatusModel
    {
        public bool IsSuccess { get; set; }
        public byte StatusId { get; set; }
        public string Result { get; set; }
    }
}