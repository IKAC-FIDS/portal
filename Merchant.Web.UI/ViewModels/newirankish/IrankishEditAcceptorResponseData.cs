using System.Collections.Generic;
using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public class IrankishEditAcceptorResponseData
    {
        public List<ErrorItem> errors  {get;set;}
        public string status { get; set;  }
    }
}