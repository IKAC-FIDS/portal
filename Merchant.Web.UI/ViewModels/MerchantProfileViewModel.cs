using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MerchantProfileViewModel
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
        public DateTime LastUpdateTime { get; set; }
        public DateTime SubmitTime { get; set; }
        public long UserId { get; set; }
        public string FatherName { get; set; }
        public string IdentityNumber { get; set; }
        public DateTime Birthdate { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public byte[] IdentityImageData { get; set; }
        public byte[] NationalCardImageData { get; set; }
        public string GenderTitle { get; set; }
        public string NationalityTitle { get; set; }
    }
}