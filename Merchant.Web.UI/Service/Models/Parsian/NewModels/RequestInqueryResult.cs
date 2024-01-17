using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RequestInqueryResult  
    {
        public  int TraceId { get; set; }
        public string Method { get; set; }
        public  bool IsSuccess { get; set; }
        public string Desc { get; set; }
        
        
        public List<ErrorList> ErrorList { get; set; }
        public  RequestResultInqueryRequest RequestResult { get; set; }
        
        
       
    }

    public class RequestResultInqueryRequest
    {
        public  string TopiarId { get; set; }
        public  string TerminalNumber { get; set; }
        public  int Stepcode { get; set; }
        public  string StepTitle { get; set; }
        public  int StatusCode { get; set; }
        public  string StatusTitle { get; set; }
        public  List<ErrorObj> RequestError { get; set; } 
        public  List<RequestDetails> RequestDetails { get; set; }
    }

    public class ErrorObj
    {
        public  string ErrorId { get; set; }
        public  string ErrorText { get; set; }
    }
}