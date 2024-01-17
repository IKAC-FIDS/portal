using System;

namespace TES.Merchant.Web.UI.Service.Models.Parsian
{
    public class TerminalInfo
    {
        public long Id { get; set; }
        public byte StateId { get; set; }
        public string MerchantNo { get; set; }
        public long DeviceTypeId { get; set; }
        public string TerminalNo { get; set; }
        public string Title { get; set; }
        public string EnglishTitle { get; set; }
        public long BranchId { get; set; }
        public string AccountNo { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public long CityId { get; set; }
        public string Tel { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public DateTime SubmitTime { get; set; }
        public long UserId { get; set; }
        public long GuildId { get; set; }
        public byte ActivityTypeId { get; set; }
        public string Business { get; set; }
        public string ShaparakAddressFormat { get; set; }
        public string FirstName { get; set; }
        public byte? RegionalMunicipalityId { get; set; }
        public string LastName { get; set; }
        public string SignatoryPosition { get; set; }
        public string EnglishFirstName { get; set; }
        public string EnglishLastName { get; set; }
        public string NationalCode { get; set; }
        public bool IsMale { get; set; }
        public string Mobile { get; set; }
        public long MarketerId { get; set; }
        public bool IsLegalPersonality { get; set; }
        public string FatherName { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public DateTime? CompanyRegistrationDate { get; set; }
        public string LegalNationalCode { get; set; }
        public DateTime BirthCertificateIssueDate { get; set; }
        public long? ParentBranchId { get; set; }
        public string TelCode { get; set; }
        public long? ParentGuildId { get; set; }
        public string EnglishFatherName { get; set; }
        public string HomeTel { get; set; }
        public string HomePostCode { get; set; }
        public string HomeAddress { get; set; }
        public string TaxPayerCode { get; set; }
        public int? TopiarId { get; set; }
        public string BirthCrtfctSeriesNumber { get; set; }
        public string BirthCrtfctSerial { get; set; }
        public string PersianCharRefId { get; set; }
    }
}