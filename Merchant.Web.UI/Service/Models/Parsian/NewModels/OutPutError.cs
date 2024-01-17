using System.Collections.Generic;

namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class OutPutError
    {
        public  int TraceId { get; set; }
        public string Method { get; set; }
        public  bool IsSuccess { get; set; }
        public string Desc { get; set; }
     
        public List<ErrorList> ErrorList { get; set; }
    }

    public class ErrorList
    {
        public  string ErrorText { get; set; }
        public  string ErrorId { get; set; }
    }
}