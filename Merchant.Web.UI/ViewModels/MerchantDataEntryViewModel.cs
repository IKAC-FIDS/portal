using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MerchantDataEntryViewModel
    {
        // MerchantProfile
        public long MerchantProfileId { get; set; }

        [Required]
        [StringLength(500)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(500)]
        public string LastName { get; set; }

        [StringLength(500)]
        public string EnglishFirstName { get; set; }

        [StringLength(500)]
        public string EnglishLastName { get; set; }

        [Required]
        [StringLength(10)]
        public string NationalCode { get; set; }

        [Required]
        public bool IsMale { get; set; }

        public string HomePostCode { get; set; }
        public string HomeTel { get; set; }

        [Required]
        public string Mobile { get; set; }

        public string HomeAddress { get; set; }

        [Required]
        public bool IsLegalPersonality { get; set; }

        [Required]
        public long NationalityId { get; set; }

        [Required]
        [StringLength(500)]
        public string FatherName { get; set; }

        [StringLength(200)]
        public string EnglishFatherName { get; set; }

        [Required]
        [StringLength(50)]
        public string IdentityNumber { get; set; }

        [Required]
        public DateTime Birthdate { get; set; }


        public string CompanyRegistrationNumber { get; set; }
        public string LegalNationalCode { get; set; }
        public DateTime? CompanyRegistrationDate { get; set; }

        // Terminal
        public long TerminalId { get; set; }
        public long DeviceTypeId { get; set; }

        [Required]
        [StringLength(32)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string EnglishTitle { get; set; }
        public long BranchId { get; set; }

        [Required]
        public long CityId { get; set; }

        public byte? RegionalMunicipalityId { get; set; }

        [Required]
        [StringLength(50)]
        public string TelCode { get; set; }

        [Required]
        [StringLength(50)]
        public string Tel { get; set; }

        public string TaxPayerCode { get; set; }

        public string Address { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        public string PostCode { get; set; }

        [Required]
        public long MarketerId { get; set; }

        [Required]
        public long GuildId { get; set; }

        [Required]
        public byte ActivityTypeId { get; set; }

        [Required]
        public string ShaparakAddressFormat { get; set; }

        public IEnumerable<DocumentDataEntryViewModel> PostedFiles { get; set; }

        [Required]
        public string AccountRow { get; set; }

        [Required]
        public string AccountCustomerNumber { get; set; }

        [Required]
        public string AccountType { get; set; }

        [Required]
        public string AccountBranchCode { get; set; }

        public string AccountNo { get; set; }
        public long? ParentGuildId { get; set; }
        public byte StateId { get; set; }
        public byte StatusId { get; set; }

        [Required]
        public DateTime BirthCertificateIssueDate { get; set; }

        public DateTime? BlockDocumentDate { get; set; }

        [StringLength(50)]
        public string BlockDocumentNumber { get; set; }

        public string BlockAccountRow { get; set; }
        public string BlockAccountCustomerNumber { get; set; }
        public string BlockAccountType { get; set; }
        public string BlockAccountBranchCode { get; set; }
        public int BlockPrice { get; set; }
        public string CustomerNumber { get; set; }

        public string SignatoryPosition { get; set; }

        public byte? PreferredPspId { get; set; }
        public int CustomerCategoryId { get; set; }

        public IEnumerable<UploadedDocumentViewModel> PreviouslyUploadedDocuments { get; set; }
        public string BirthCrtfctSerial { get; set; }
        public string PersianCharRefId { get; set; }
        public string BirthCrtfctSeriesNumber { get; set; }
    }
}