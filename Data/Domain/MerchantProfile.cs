using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TES.Data.Domain
{
    [Table("psp.MerchantProfile")]
    public class MerchantProfile
    {
        public long Id { get; set; }

        //[Required]
        [StringLength(500)]
        public string FirstName { get; set; }

        //[Required]
        [StringLength(500)]
        public string LastName { get; set; }

        [StringLength(500)]
        public string EnglishFirstName { get; set; }

        [StringLength(500)]
        public string EnglishLastName { get; set; }

        //[Required]
       
        public string NationalCode { get; set; }

        [StringLength(50)]
        public string HomeTel { get; set; }

        //[Required]
        [StringLength(50)]
        public string Mobile { get; set; }

        [StringLength(2000)]
        public string HomeAddress { get; set; }

        [StringLength(10)]
        public string HomePostCode { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime LastUpdateTime { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime SubmitTime { get; set; }

        public long UserId { get; set; }

        public long NationalityId { get; set; }

        //[Required]
        [StringLength(500)]
        public string FatherName { get; set; }

        [Required]
        [StringLength(50)]
        public string IdentityNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birthdate { get; set; }

        [StringLength(50)]
        public string CompanyRegistrationNumber { get; set; }

        [Column(TypeName = "date")]
        public DateTime? CompanyRegistrationDate { get; set; }

       
        public string LegalNationalCode { get; set; }

        [Column(TypeName = "date")]
        public DateTime BirthCertificateIssueDate { get; set; }

        public string SignatoryPosition { get; set; }

        [StringLength(200)]
        public string EnglishFatherName { get; set; }

        public bool IsLegalPersonality { get; set; }
        public bool IsMale { get; set; }
        public string CustomerNumber { get; set; }

        public virtual Nationality Nationality { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<MerchantProfileDocument> MerchantProfileDocuments { get; set; } = new HashSet<MerchantProfileDocument>();
        public virtual ICollection<Terminal> Terminals { get; set; } = new HashSet<Terminal>();
        public string BirthCrtfctSerial { get; set; }
        public string BirthCrtfctSeriesNumber { get; set; }
        public  string PersianCharRefId { get; set; }
        public int? PardakhtNovinCustomerId { get; set; }
    }
}