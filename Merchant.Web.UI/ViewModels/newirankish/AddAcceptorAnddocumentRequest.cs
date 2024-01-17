using System;
using System.Collections.Generic;
using TES.Merchant.Web.UI.IranKishServiceRefrence;
using TES.Merchant.Web.UI.ViewModels.PardakhtNovin;

namespace TES.Merchant.Web.UI.ViewModels.newirankish
{
    internal sealed class AddAcceptorAnddocumentRequest
    {
        public List<irankishDocument > Documents  { get; set; } //ok
        public List<AccountEntity> accounts { get; set; }  //ok

        public EntityTypes EntityType { get; set; }  //ok
        public string BusinessCertificateNumber { get; set; } //ok

        public string IdentifierNumber { get; set; }   //ok

        public string IdentifierSerial { get; set; }  //ok

        public string IdentifierLetterPart { get; set; } //ok

        public int IdentifierNumberPart { get; set; }   //ok

        public int Group { get; set; }  //ok

        public decimal BankId { get; set; }  //ok

        public string CertificateIssueDate { get; set; }//ok

        public string CertificateExpireDate { get; set; }//ok
  

        public string FirstName { get; set; }  //ok

        public string LastName { get; set; }  //ok

        public string LegalEntityTitle { get; set; }//ok

        public string LegalNationalId { get; set; }//ok
      
        public string RealNationalId { get; set; }  //ok

        public string PervasiveId { get; set; }//ok

        public string PassportId { get; set; }//ok

        public string PassportExpireDate { get; set; }//ok

        public string LicenseId { get; set; }//ok

        public string LicensorOrg { get; set; }//ok

        public string Nationality { get; set; }  //ok

        public string Mobile { get; set; }  //ok

        public string Phone { get; set; }  //ok

        public string Zipcode { get; set; }  //ok

        public string Address { get; set; }  //ok

        public AcceptorTypes AcceptorType { get; set; }  //ok

        public string Bussiness { get; set; }  //ok

        public string Activity { get; set; }  //ok


      
        public string Province { get; set; }  //ok

        public string City { get; set; }  //ok

        public string TerminalType { get; set; }  //ok

        public int Qty { get; set; }  //ok

        public bool IsPcPos { get; set; }  //ok

        public string TrackId { get; set; } //ok 

        public string Email { get; set; }//ok

        public string WebUrl { get; set; }//ok

        public string MerchantName { get; set; }  //ok

        public bool IsSwitchTerminal { get; set; }  //ok

        public DateTime? AcceptorCeoBirthdate { get; set; }  //ok

        public string FoundationDate { get; set; }//ok

        public string TerminalId { get; set; }//ok

        public string AcceptorId { get; set; }//ok

        public string DepositID { get; set; }//ok

        public string TechFirstName { get; set; }//ok

        public string TechLastName { get; set; }//ok

        public string TechMobile { get; set; }//ok

        public bool ENamadStatus { get; set; }  //ok

        public string ProductInfo { get; set; }//ok

        public string IpAddress { get; set; }  //ok

        public string AsanKharidFromDate { get; set; }//ok

        public string AsanKharidToDate { get; set; }//ok

        public int? AsanKharidPayFrom { get; set; }//ok

        public int? AsanKharidPayTo { get; set; }//ok

        public string AsanKharidWagePercent { get; set; }//ok

        public bool IsAsankharid { get; set; }//ok

        public string ContractType { get; set; }  //ok

        public string MposRrn { get; set; }//ok

        public string TaxFollowupCode { get; set; }  //ok

        public string LicenseNumber { get; set; }//ok

        public string TechEmail { get; set; }//ok

        public string AsankharidAgencyWorker { get; set; }//ok

        public string AsankharidAgencyWorkerCode { get; set; }//ok
        public bool IsVirtual  { get; set; }//ok

    }
}