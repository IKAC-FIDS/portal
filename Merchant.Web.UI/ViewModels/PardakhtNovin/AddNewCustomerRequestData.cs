using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewCustomerRequestData
    {
        public  string BCID { get; set; }
        public  DateTime BirthDate { get; set; }
        public  string CompanyCode { get; set; }
        public  DateTime? CompanyFoundationDate { get; set; }
        public  string     CompanyName { get; set; }
        public  int     CustomerTypeID { get; set; }
        public  string     Email { get; set; }
        public  string     FatherName { get; set; }
        public  string     FirstName { get; set; }
        public  string     ForeignersPervasiveCode { get; set; }
        public bool ISForeignNationals { get; set; } = false;
        public  string     Mobile { get; set; }
        public  string     NationalCode { get; set; }
        public  string     PassportCode { get; set; }
        public  string     FirstNameEN  { get; set; }
        public  string     LastNameEN   { get; set; }
        public  string     FatherNameEn    { get; set; }
        public  string     CompanyNameEn     { get; set; }
        public  string     CompanyRegisterNo      { get; set; }
        public  int     GenderID       { get; set; }
        public  DateTime? PassportCreditDate  { get; set; }
        public  string Country   { get; set; }
        public string LastName { get; set; }
        

    }
}