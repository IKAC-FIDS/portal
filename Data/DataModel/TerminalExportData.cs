using System;

namespace TES.Data.DataModel
{
    public class TerminalExportData
    {
        public long TerminalId { get; set; }
        public long MerchantProfileId { get; set; }
        public string DeviceTypeTitle { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public  int InstallDelayDays { get; set; }
        public string StatusTitle { get; set; }
        public string MerchantNo { get; set; }
        public string TerminalNo { get; set; }
        public string TerminalTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EnglishFirstName { get; set; }
        public string EnglishLastName { get; set; }
        public string NationalCode { get; set; }
        public string AccountNo { get; set; }
        public string CityTitle { get; set; }
        public string StateTitle { get; set; }
        public string Mobile { get; set; }
        public string PspTitle { get; set; }
        public string SubmitterUserFullName { get; set; }
        public long MarketerId { get; set; }
        public long SumOfTransactions { get; set; }
        public long BranchId { get; set; }
        public string BranchTitle { get; set; }
        public DateTime SubmitTime { get; set; }
        public DateTime? InstallationDate { get; set; }
        public DateTime? RevokeDate { get; set; }
        public string ErrorComment { get; set; }
        public string ParentBranchTitle { get; set; }
        public string Address { get; set; }
        public string TelCode { get; set; }
        public string Tel { get; set; }
        public string PostCode { get; set; }
        public string ParentGuildTitle { get; set; }
        public bool IsLegalPersonality { get; set; }
        public string LegalPersonalityTitle => IsLegalPersonality ? "حقوقی" : "حقیقی";
        public DateTime? Birthdate { get; set; }
        public string GuildTitle { get; set; }
        public string IdentityNumber { get; set; }
        public  bool? IsGood { get; set; }
        public double? IsGoodValue { get; set; }
        public string FatherName { get; set; }
        public string EnglishFatherName { get; set; }
        public string LegalNationalCode { get; set; }
        public DateTime? CompanyRegistrationDate { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string MarketerTitle { get; set; }
        public DateTime? BatchDate { get; set; }
        public bool IsMale { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public string ContractNo { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? BlockDocumentDate { get; set; }
        public string BlockDocumentNumber { get; set; }
        public string BlockAccountNumber { get; set; }
        public int? BlockPrice { get; set; }
        public string TransactionStatusText { get; set; }
        public string TaxPayerCode { get; set; }
        
        public  string HomePostCode { get; set; }
        public  string HomeAddress { get; set; }
        
        public  bool? LowTransaction { get; set; }

   

        public bool? IsActive { get; set; }
 
        //public bool IsVip { get; set; }
    }
}