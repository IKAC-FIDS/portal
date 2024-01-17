using System.Collections.Generic;
using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public class NewIrankishAddAcceptorResposneData
    {
        public  int id { get; set; }
        public  List<ErrorItem > errors { get; set; }
        public  string psptrackingCode { get; set; }
        public  string documentTrackingCode { get; set; }
        public  string trackingcode { get; set; }
        public  string indicator { get; set; }
        public  string status { get; set; }

    }
}