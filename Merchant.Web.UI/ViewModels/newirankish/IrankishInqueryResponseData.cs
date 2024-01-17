using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public class IrankishInqueryResponseData
    {
        public  int id { get; set; }
        public  string acceptor { get; set; }
        public  DateTime requestInsertDate { get; set; }
        public  int status { get; set; }
        public  string accountStatusDescription { get; set; }
        public  int accountStatus { get; set; }
        public  string accountNo { get; set; }
        public  DateTime penCodeDate { get; set; }
        public  string documentTrackingCode { get; set; }
        public  string indicator { get; set; }
        public  string documnentStatus { get; set; }
        public  string description { get; set; }
        public  List<newIrankishterminal> terminal { get; set; }
    }
}