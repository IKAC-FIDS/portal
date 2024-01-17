using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class DocumentDataEntryViewModel
    {
        public int ForEntityTypeId { get; set; }
        public long DocumentTypeId { get; set; }
        public HttpPostedFileBase PostedFile { get; set; }
        public bool IsRequired { get; set; }
        public bool? IsForLegalPersonality { get; set; }
    }
}