using System.Collections.Generic;

namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RequestTerminalّForCompanyInput : ParsianInput
    { 
        public  RequestTerminalForCompanyRequest RequestData { get; set; }

      
    }

    public class UploadAttachmentInput : ParsianInput
    {
      
        
        public  UploadAttachmentRequestData RequestData { get; set; }

    }

    public class UploadAttachmentRequestData
    {
        public  string ContentType { get; set; }
        public  string Base64 { get; set; }
        public  string FileName { get; set; }
        public bool CanUpload { get; set; }
    }
    public class RequestTerminalّForPersonInput : ParsianInput
    { 
        public  RequestTerminalForPersonRequest RequestData { get; set; }
      //  public List<UploadAttachmentRequestData> Files { get; set; }
    }
    public class RequestTerminalForPersonRequest  
    {
        public Request Request { get; set; }
        public  Person Person { get; set; }         
        public  Shop Shop { get; set; }
        public  Settlements Settlements { get; set; }
        public  List<Attachment> Attachments { get; set; }

    }
    
    public class RequestTerminalForCompanyRequest  
    {
        public Request Request { get; set; }    
        public Company Company { get; set; }
        public  List<CompanyOwners> CompanyOwners { get; set; }
        public  Shop Shop { get; set; }
        public  Settlements Settlements { get; set; }
       
        public  List<Attachment> Attachments { get; set; }


    }
}