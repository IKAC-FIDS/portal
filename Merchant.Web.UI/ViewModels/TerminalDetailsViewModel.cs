using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class TerminalDetailsViewModel
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalCode { get; set; }
        public bool IsMale { get; set; }
        public string HomeTel { get; set; }
        public string HomePostCode { get; set; }
        public string Mobile { get; set; }
        public string HomeAddress { get; set; }
        public bool IsLegalPersonality { get; set; }
        public string LegalPersonalityTitle { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime SubmitTime { get; set; }
        public long UserId { get; set; }
        public string NationalityTitle { get; set; }
        public string FatherName { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string GenderTitle { get; set; }
        public string ActivityTypeTitle { get; set; }
        public long TerminalId { get; set; }
        public string ErrorComment { get; set; }
        public string MerchantNo { get; set; }
        public long? DeviceTypeId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string TerminalNo { get; set; }
        public string Title { get; set; }
        public long BranchId { get; set; }
        public string BranchTitle { get; set; }
        public string AccountNo { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public string StatusTitle { get; set; }
        public byte? PspId { get; set; }
        public string PspTitle { get; set; }
        public DateTime? BatchDate { get; set; }
        public long CityId { get; set; }
        public string CityTitle { get; set; }
        public string StateTitle { get; set; }
        public byte? RegionalMunicipalityId { get; set; }
        public string RegionalMunicipalityTitle { get; set; }
        public string TelCode { get; set; }
        public string Tel { get; set; }
        public string Address { get; set; }
        public string ShaparakAddressFormat { get; set; }
        public string PostCode { get; set; }
        public long MarketerId { get; set; }
        public string MarketerTitle { get; set; }
        public string ContractNo { get; set; }
        public DateTime? ContractDate { get; set; }
        public long GuildId { get; set; }
        public string GuildTitle { get; set; }
        public long MerchantProfileId { get; set; }
        public string SubmitterUserFullName { get; set; }
        public DateTime BirthCertificateIssueDate { get; set; }
        public string SignatoryPosition { get; set; }
        public string EnglishFirstName { get; set; }
        public string EnglishLastName { get; set; }
        public DateTime? BlockDocumentDate { get; set; }
        public string BlockDocumentNumber { get; set; }
        
        public  byte?  BlockDocumentStatusId { get; set; }
        public string BlockAccountNumber { get; set; }
        public int? BlockPrice { get; set; }
        public string PreferredPspTitle { get; set; }
        public  int? FanavaTerminals { get; set; }
        public  int? IrankishTerminals { get; set; }
        public  int? ParsianTerminals { get; set; }
        public string LegalNationalCode { get; set; }
        public DateTime? CompanyRegistrationDate { get; set; }
        public string TaxPayerCode { get; set; }

        public IEnumerable<DocumentViewModel> TerminalDocuments { get; set; }
        public IEnumerable<DocumentViewModel> MerchantProfileDocuments { get; set; }
        public int? TopiarId { get; set; }
        public int? StepCode { get; set; }
        public string StepCodeTitle { get; set; }
        public string InstallStatus { get; set; }
        public int? InstallStatusId { get; set; }
        public bool? NewParsian { get; set; }
        public int? CustomerCategoryId { get; set; }
        public string CustomerCategory { get; set; }
        public long OrgaNizationId { get; set; }
        public double? Wage { get; set; }
    }


    public class ReadTransactionWageDto
    {
       
        public string RRN { get; set; }
        public  string RowNumber { get; set; }
        public bool? HasError { get; set; }
        public  string TerminalNo { get; set; }
        public  double WageValue { get; set; }
        public string Error { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Sheba { get; set; }
    }
    
}