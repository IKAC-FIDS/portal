namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RequestRevocationTerminalInput : ParsianInput
    {
        public  RequestRevocationTerminalRequestData RequestData { get; set; }
    }

    public class RevokeRequestResult : OutPutError
    {
        public  abc RequestResult { get; set; } 
    }

    public class abc
    {
        public  string ResultText { get; set; }
        public  int TopiarId { get; set; }
    }
}