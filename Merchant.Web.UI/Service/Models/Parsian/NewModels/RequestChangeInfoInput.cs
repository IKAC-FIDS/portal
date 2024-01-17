using System.Collections.Generic;

namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RequestChangeInfoInput : ParsianInput
    {
        public  RequestChangeInfoInputData RequestData { get; set; }

    }

    public class RequestChangeShopPost : ParsianInput
    {
        public  RequestChangeShopPostData  RequestData { get; set; }

    }
    public class RequestChangeInfoInput2 : ParsianInput
    {
        public  RequestChangeInfoInputData2 RequestData { get; set; }

    }
    public class RequestChangeAccountInfoInput : ParsianInput
    {
        public  RequestChangeAccountInfoInputData RequestData { get; set; }

    }

    public class RequestChangeAccountInfoResult : OutPutError
    {
        public  RequestChangeAccountInfoResultRequestData RequestResult { get; set; }

    }
  

    public class RequestChangeAccountInfoResultRequestData
    {
        public  string TopiarId { get; set; }
    }
    public class RequestChangeAccountInfoInputData
    {         
        public  string AcceptorCode { get; set; }    
        public  string TaxPayerCode { get; set; }
        public  List<IbanData > Ibans { get; set; } 
        public  List<string> Terminals { get; set; }
        public  List<Attachment> Attachments { get; set; }
    }

}