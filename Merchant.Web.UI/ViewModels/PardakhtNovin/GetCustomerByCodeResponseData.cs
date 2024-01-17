using System;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class GetCustomerByCodeResponseData
    {
        public  string BCID { get; set; }
        public  DateTime BirthDate { get; set; }
        public  string BirthPlace { get; set; }
        public  string CompanyCode { get; set; }
        public  string CompanyFoundationDate { get; set; }
        public  string CompanyName { get; set; }
        public  string CustomerCode { get; set; }
        public  string CustomerType { get; set; }
        public  int CustomerTypeID { get; set; }
        public  int CustomerID { get; set; }
        public  string Email { get; set; }
        public  string FatherName { get; set; }
        public  string FirstName { get; set; }
        public  string ForeignersPervasiveCode { get; set; }
        public  bool ISForeignNationals { get; set; }
        public  string LastName { get; set; }
        public  string Mobile { get; set; }
        public  string NationalCode { get; set; }
        public  string PassportCode { get; set; }
        public  DateTime? PassportCreditDate { get; set; }

    }
}