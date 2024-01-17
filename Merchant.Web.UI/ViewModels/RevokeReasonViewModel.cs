namespace TES.Merchant.Web.UI.ViewModels
{
    public class RevokeReasonViewModel
    {
        public byte Id { get; set; }
        public string Title { get; set; }
        public byte Level { get; set; }
        public byte? ParentId { get; set; }
    }
    
    public class CustomerCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int From { get; set; }

        public int To { get; set; }

    }
}