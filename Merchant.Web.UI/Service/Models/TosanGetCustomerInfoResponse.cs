using System;

namespace TES.Merchant.Web.UI.Service.Models
{
    public class TryGetCustomerInfoInput
    {
        public string selectedAccountCustomerNumber { get; set; }
        public string primaryAccountCustomerNumber { get; set; }
    }
    public class TosanGetCustomerInfoResponse
    {
        public DateTime Birthdate { get; set; }
        public string NationalCode { get; set; }
        public string FatherLatinName { get; set; }
        public string FatherName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LatinFirstName { get; set; }
        public string LatinLastName { get; set; }
        public string Mobile { get; set; }
        public string IdentityNumber { get; set; }
        public bool IsLegalPersonality { get; set; }
        public bool IsMale { get; set; }
        public bool BirthDateFieldSpecified { get; set; }
        public bool GenderFieldSpecified { get; set; }
        public bool PersonalityTypeFieldSpecified { get; set; }
        public Address HomeAddress { get; set; }
        public string LegalNationalCode { get; set; }
        public DateTime? CompanyRegistrationDate { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public DateTime? PrimaryAccountBirthDate { get; set; }
        public string certificateSeries { get; set; }
        public string certificateSerial { get; set; }
        public string cif { get; set; }
        public string birthLocationCode { get; set; }

        public class Address
        {
            public string PostalAddress { get; set; }
            public string PhoneNumber { get; set; }
            public string PostalCode { get; set; }
        }
    }
}