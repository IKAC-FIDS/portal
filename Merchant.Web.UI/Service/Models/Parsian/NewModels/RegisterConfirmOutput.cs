using System.Collections.Generic;

namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RegisterConfirmOutput : OutPutError
    {
        public  List<NewParsianTerminal>  Terminals { get; set; }
    }
}