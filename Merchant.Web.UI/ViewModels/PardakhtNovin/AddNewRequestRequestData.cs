using System;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
   
    public class AddNewRequestRequestData
    {
     
    public string BankFollowupCode { get; set; }
        public  string PortType { get; set; }
        public  string PosType { get; set; }
        public  string PosModel { get; set; }
        public  string AccountNumber { get; set; }
    //    public  string ActivityLicenseNumberReferenceName{ get; set; }
        public  DateTime? BusinessLicenseEndDate{ get; set; }
        public  int BankID{ get; set; }
        public  string BranchCode { get; set; }
        public  string FaxNumber { get; set; }
        public  string WorkTitle { get; set; }
        public  string WorkTitleEng { get; set; }
        public  string ShaparakAddressText { get; set; } 
        public  DateTime? RentEndingDate { get; set; } 
        public  string PostalCode { get; set; } 
        public  string PhoneNumber { get; set; } 
        public  string Mobile { get; set; } 
        public  int MainCustomerID { get; set; } 
      //  public  string ActivityLicenseNumber { get; set; }
        public  string AccountShabaCode { get; set; }
        public  string CityShaparakCode { get; set; }
        public  string GuildSupplementaryCode  { get; set; }
        public  int OwneringTypeID   { get; set; } 
        public  string RentContractNo   { get; set; } 
        public  string HowToAssignID  { get; set; }
        public  string TrustKind   { get; set; }
        public  string TrustNumber   { get; set; }
        public  string CityZone  { get; set; }
        public  string CashTrustRRN   { get; set; }
        public  string TaxPayerCode { get; set; }
        public  string WorkTypeID    { get; set; }
        public  string MobileGPRS     { get; set; } 

        public  bool? IsMultiAccount     { get; set; } 
        public  bool? IsMultiAccountOwner     { get; set; } 
        public  int ?MainSharingPercent     { get; set; }
        public string ActivityLicenseNumberReferenceName { get; set; }
        public string ActivityLicenseNumber { get; set; }
       
    }
}