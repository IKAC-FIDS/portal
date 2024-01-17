namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class TerminalInQueryOutPut : OutPutError
    {
        
        public  TerminalInQueryRequestResult RequestResult { get; set; }
     

        

    }

    public class TerminalInQueryRequestResult
    {
        public  string AssignSerialNumber { get; set; }
        public  string InstallDate { get; set; }

        public  TerminalStatusCode StatusCode { get; set; }

        public  string StatusTitle { get; set; }

        public  string UnistallDate { get; set; }
    }

    public enum TerminalStatusCode
    {
        ersalbenamayandegi = 0,
        takhsisyafteh  = 1,
        jahavarishodeh = 2,
        tahteTamir,
        rahandazishodeh,
        outeOfService,
        radehKharej,
        readyForAssign,
        readyForFaskh
        
    }
}