namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    public sealed class NewChangeAccountRequest
    {
        public string CurrentIban { get; set; }

        public string NewAccountNo { get; set; }

        public string NewIbanNo { get; set; }

        public int? AccountType { get; set; }

        public string AcceptorNo { get; set; }

        public string ChangeAccountDocument { get; set; }

        public string BirthCertificate { get; set; }

        public string NationalDocument { get; set; }

        public string ChangeAccountDescription { get; set; }

        public string OwnerFamily { get; set; }

        public string OwnerName { get; set; }

        public string BranchCode { get; set; }
    }
}