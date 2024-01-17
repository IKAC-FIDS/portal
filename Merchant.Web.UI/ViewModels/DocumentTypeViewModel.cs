namespace TES.Merchant.Web.UI.ViewModels
{
    public class DocumentTypeViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool IsRequired { get; set; }
        public int ForEntityTypeId { get; set; }
        public bool? IsForLegalPersonality { get; set; }
    }
}