using System;
using TES.Merchant.Web.UI.IranKishServiceRefrence;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    internal sealed class AddAcceptorRequest
    {
        public string BusinessCertificateNumber { get; set; }

        public string IdentifierNumber { get; set; }

        public string  IdentifierSerial { get; set; }

        public string  IdentifierLetterPart { get; set; }

        public int IdentifierNumberPart { get; set; }

        public int Group { get;  set; }

        public decimal BankId { get; set; }

        public string  CertificateIssueDate { get; set; }

        public string  CertificateExpireDate { get;  set; }

        public EntityTypes EntityType { get;  set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public  string LegalEntityTitle { get; set; }

        public  string LegalNationalId { get; set; }

        public string RealNationalId { get; set; }  

        public  string PervasiveId { get; set; }

        public  string PassportId { get; set; }

        public  string PassportExpireDate { get; set; }

        public  string LicenseId { get; set; }

        public  string LicensorOrg { get; set; }

        public string Nationality { get; set; }  

        public string Mobile { get; set; }  

        public string Phone { get; set; }  

        public string Zipcode { get; set; }  

        public string Address { get; set; }  

        public AcceptorTypes AcceptorType { get; set; }  

        public string Bussiness { get; set; }  

        public string Activity { get; set; }  

        public string Account { get; set; }  

        public string Iban { get; set; }  

        public string Branch { get; set; }  

        public string Province { get; set; }  

        public string City { get; set; }  

        public string TerminalType { get; set; }  

        public int Qty { get; set; }  

        public bool IsPcPos { get; set; }  

        public string TrackId { get; set; }  

        public  string Email { get; set; }

        public  string WebUrl { get; set; }

        public string MerchantName { get; set; }  

        public bool IsSwitchTerminal { get; set; }  

        public DateTime? AcceptorCeoBirthdate { get; set; }  

        public  DateTime? FoundationDate { get; set; }

        public  string TerminalId { get; set; }

        public  string AcceptorId { get; set; }

        public  string DepositID { get; set; }

        public  string TechFirstName { get; set; }

        public  string TechLastName { get; set; }

        public  string TechMobile { get; set; }

        public bool ENamadStatus { get; set; }  

        public  string ProductInfo { get; set; }

        public string IpAddress { get; set; }  

        public  string AsanKharidFromDate { get; set; }

        public  string AsanKharidToDate { get; set; }

        public int? AsanKharidPayFrom { get; set; }

        public int? AsanKharidPayTo { get; set; }

        public  string AsanKharidWagePercent { get; set; }

        public bool IsAsankharid { get; set; }

        public  string ContractType { get; set; }  

        public  string MposRrn { get; set; }

        public string TaxFollowupCode { get; set; }  

        public  string LicenseNumber { get; set; }

        public  string TechEmail { get; set; }

        public  string AsankharidAgencyWorker { get; set; }

        public  string AsankharidAgencyWorkerCode { get; set; }
        public bool IsVirtual { get; set; }
    }

    public class AccountEntity
{
    public long OrderNo { get; set; }
    public string Bank { get; set; }
    public string Branch { get; set; } 
    public string Account { get; set; } 
    public string Iban { get; set; }
    public string OwnerName { get; set; }
    public string OwnerFamily { get; set; }
    public string DiscountPercent { get; set; }
    public string DistributionAmount { get; set; }
}
}