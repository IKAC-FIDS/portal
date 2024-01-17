namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class AssignDeviceToTerminalRequestData
    {
        public  string TerminalNo { get; set; }
        public  string SerialNo { get; set; }
        public int DeviceModelRefId { get; set; } = 2;
        public  int RequestorRefId { get; set; }
        public  bool RemoveFromOldTerminal { get; set; }
    }
}