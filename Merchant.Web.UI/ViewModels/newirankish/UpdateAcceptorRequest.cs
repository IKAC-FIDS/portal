using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    internal sealed class UpdateAcceptorRequest
    {
        public string MposRrn { get; set; }

        public string Description { get; set; }

        public EntityTypes EntityType { get; set; }  

        public string IdentifierNumber { get; set; }

        public string IdentifierSerial { get; set; }

        public string IdentifierLetterPart { get; set; }

        public int? IdentifierNumberPart { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string LegalEntityTitle { get; set; }

        public string LegalNationalId { get; set; }

        public string RealNationalId { get; set; }

        public string PervasiveId { get; set; }

        public string PassportId { get; set; }

        public string PassportExpireDate { get; set; }

        public string LicenseId { get; set; }

        public string LicensorOrg { get; set; }

        public string Nationality { get; set; }

        public string Mobile { get; set; }

        public string Phone { get; set; }

        public string Zipcode { get; set; }

        public string Address { get; set; }

        public AcceptorTypes AcceptorType { get; set; }

        public string Bussiness { get; set; }

        public string Activity { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string Email { get; set; }

        public string WebUrl { get; set; }

        public string MerchantName { get; set; }

        public string AcceptorCeoBirthdate { get; set; }

        public string FoundationDate { get; set; }

        public string TechEmail { get; set; }

        public string TechFirstName { get; set; }

        public string TechLastName { get; set; }

        public string TechMobile { get; set; }

        public bool ENamadStatus { get; set; }

        public string ProductInfo { get; set; }

        public string TaxFollowupCode { get; set; }

        public decimal BankId { get; set; }  

        public string AccountNo { get; set; }

        public string Iban { get; set; }

        public string AcceptorNo { get; set; }

        public string LicenseNumber { get; set; }

        public string IpAddress { get; set; }

        public int Qty { get; set; }

        public bool IsPcPos { get; set; }

        public string TrackId { get; set; }  

        public bool IsSwitchTerminal { get; set; }
    }
}