namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class TerminalResult : OutPutError
    {
        public  TerminalRequestResult  RequestResult { get; set; }
      
    }

    public class UploadAttachmentResult :OutPutError
    {
        public  UploadAttachmentRequestResult RequestResult { get; set; }
     
    }

    public class UploadAttachmentRequestResult
    {
        public  string FileRef { get; set; }
    }
    public class TerminalRequestResult
    {
        public  int? TopiarId { get; set; }
    }
    
}