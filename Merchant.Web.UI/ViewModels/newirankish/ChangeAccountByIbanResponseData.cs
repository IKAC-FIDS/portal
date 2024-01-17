using System.Collections.Generic;
using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public class ChangeAccountByIbanResponseData
    {
     
        public bool Status { get; set; }
        public List<ErrorItem> Errors
        { get; set; }
    }
}