using System.Collections.Generic; 
namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class RequestChangeInfoInputData
    {
        public int  ChangeInfoTypeRefId { get; set; }
        public int PersonTypeRefId { get; set; }
        public string NationalCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CommercialCode { get; set; }
        public string RegisterNumber { get; set; }
        public string FirstNameEn { get; set; }
        public string LastNameEn { get; set; }
        public string BirthDate { get; set; }
        public string FatherName { get; set; }
        public string FatherNameEn { get; set; }
        public GenderType  GenderTypeRefId { get; set; }
        public string BirthCertificateNumber { get; set; }
        public string CityBirth { get; set; }
        public string StoreName { get; set; }
        public string StoreNameEn { get; set; }
        public string ShopCityRef { get; set; }
        public string PostalAddress { get; set; }
        public BusinessType BusinessTypeRefId { get; set; }
        public string WebSiteAddress { get; set; }
        public string WebSitePort { get; set; }
        public string ShopEmailAddress { get; set; }
        public string BusinessCertificateNumber { get; set; }
        public string BusinessCertificateIssueDate { get; set; }
        public string BusinessCertificateExpiryDate { get; set; }
        public string OwnerShipTypeRefId { get; set; }
        public string RentalContractNumber { get; set; }
        public string RentalExpiryDate { get; set; }
        public ETrustCertificateType ETrustCertificateTypeRefId { get; set; }
        public string ETrustCertificateIssueDate { get; set; }
        public string ETrustCertificateExpiryDate { get; set; }
        public string CallBackAddress { get; set; }
        public string CallBackPort { get; set; }
        public int BirthCertificateSeriesNumber { get; set; }
        public string PassportExpireDate { get; set; }
        public string HomeTelephoneNumber { get; set; }
        public string CellPhoneNumber { get; set; }
        public string HomePostalCode { get; set; }
        public string WorkTelephoneNumber { get; set; }
        public string WorkPostalCode { get; set; }
        public string HomeAddress { get; set; }
        public string WorkAddress { get; set; }
        public string EmailAddress { get; set; }
        public string Website { get; set; }
        public string Fax { get; set; }
        public VitalStatus    VitalStatusRefId { get; set; }
        public string TaxPayerCode { get; set; }
        public string TerminalTitle { get; set; }
        public string TerminalTitleEn { get; set; }
        public string TelephoneNumber { get; set; }
        public string CellphoneNumber { get; set; }
        public string TerminalPostalCode { get; set; }
        public string TerminalPostalAddress { get; set; }
        public string TerminalNumber { get; set; }
        public int MinTransactionAmount { get; set; }
        public int MaxTransactionAmount { get; set; }
        public string Term_AccessAddress { get; set; }
        public string Term_CallBackAddress { get; set; }
        public string AcceptorCode { get; set; }
        public string TelNumber { get; set; }
        public string ShopSubMccRefId { get; set; }
        public string PostalCode { get; set; }
        public List<IbanData> Ibans { get; set; }
        public List<Attachment> Attachments { get; set; }
        public string BirthCertificateSeriesLetter { get; set; }
    }

    public class RequestChangeShopPostData
    {
        public int  ChangeInfoTypeRefId { get; set; }
        public  string TaxPayerCode { get; set; }
        public string AcceptorCode { get; set; }
    }
 public class RequestChangeInfoInputData2
    {
        public int  ChangeInfoTypeRefId { get; set; }
        public int PersonTypeRefId { get; set; }
        public string NationalCode { get; set; }
       
        public string BirthCertificateNumber { get; set; }
        
        public  string BirthDate { get; set; }
        public string  BirthCertificateSeriesNumber { get; set; }
        
        public  string CellPhoneNumber  {get;set;}
      
       
        public int PersianCharRefId { get; set; }
    }
    public enum ETrustCertificateType
    {
        oneStar = 31373,
        twoStar = 31374
    }

    public enum BusinessType
    {
        justPhysical = 31376,
        PhysicalVirtual  = 31377,
        JustVirtual = 31378
    }
    public enum ChangeInfoType
    {
        identityInfo  = 31286 , //تغییر اطلاعات هویتی
        shopechangeinfo = 31288, //تغییر اطلاعات فروشگاه
        terminalchangeinfo = 31390, //تغییر اطلاعات پایانه
        changecodeposti = 31399, //تغییر کد پستی فروشگاه
        changeshopSenf = 31400, //تغییر صنف فروشگاه
        changeTerminalSpecification = 31759 , //تغییر خصوصیات پایانه
        changeTerminalWebsite = 31762 , //تغییر وب سایت پایانه
    }
    public enum VitalStatus
    {
        alive = 1890,
        died  = 1891
    }
}