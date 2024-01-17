namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class TerminalInqueryInput : ParsianInput
    {
        public  TerminalInqueryRequestData RequestData { get; set; }
    }
    
    public class RequestInqueryInput : ParsianInput
    {
        public  RequestInqueryRequestData RequestData { get; set; }
    }
    
    public class RequestInqueryRequestData
    {
        public  string TopiarId { get; set; }
    }
}