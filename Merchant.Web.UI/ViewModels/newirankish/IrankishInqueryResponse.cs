using System.Collections.Generic;
using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public class AuthenTicationResponse
    {
        public  bool status { get; set; }
        public  AuthenTicationResponseData data { get; set; }

    }

    public class AuthenTicationResponseData
    {
        public  string jwtToken { get; set; }
    }
    public class IrankishInqueryResponse
    {
        public  int id { get; set; }
        public  bool status { get; set; }
        public  bool responseCode { get; set; }
        public  IrankishInqueryResponseData data { get; set; }

    }

    public class AccountInquiryResponse
    {
        public  int id { get; set; }
        public  bool status { get; set; }
        public  bool responseCode { get; set; }
        public  AccountInquiryResponseData data { get; set; }
         
    }

    public class AccountInquiryResponseData
    {
        public string requestStatus { get; set; }
        public string requestStatusDescription  { get; set; }
        public List<NewAccountList> accountList { get; set; }
    }

    public class NewAccountList
    {
        public string accountNo { get; set; }
        public string statusDesc { get; set; }
        public int status  { get; set; }
        public string iban { get; set; } 
    }
}