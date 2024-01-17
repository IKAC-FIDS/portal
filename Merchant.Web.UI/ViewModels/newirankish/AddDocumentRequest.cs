namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public sealed class AddDocumentRequest
    {
        public string DocumentType { get; set; }  
        public decimal BankId { get; set; } 

        public string TrackingCode { get; set; }  

        public string File { get; set; }
    }
}