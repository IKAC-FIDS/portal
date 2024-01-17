using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class TempReportDataImportViewModel
    {
        public short Year { get; set; }
        public byte Month { get; set; }

        /// <summary>
        /// تعداد دستگاه سطر 1و2
        /// </summary>
        public HttpPostedFileBase Report1And2File { get; set; }

        /// <summary>
        /// پایانه شعبه ای سطر 3
        /// </summary>
        public HttpPostedFileBase Report3File { get; set; }

        /// <summary>
        /// جریمه اخذ ترمینال سطر 4
        /// </summary>
        public HttpPostedFileBase Report4File { get; set; }

        /// <summary>
        /// جریمه تاخیر در نصب سطر 5
        /// </summary>
        public HttpPostedFileBase Report5File { get; set; }

        /// <summary>
        /// em -سطر 6
        /// </summary>
        public HttpPostedFileBase Report6File { get; set; }

        /// <summary>
        /// سطر7 -pm
        /// </summary>
        public HttpPostedFileBase Report7File { get; set; }
        
        public HttpPostedFileBase Report8File { get; set; }

        
    }
}