namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class Person
    {
        public string NationalCode { get; set; }
        public  string FirstName { get; set; }
        public  string LastName { get; set; }
        public  GenderType  GenderTypeRefId { get; set; }
        public  string CityBirth { get; set; }

        public  string BirthDateDt { get; set; }
        public string IssueDateDt { get; set; }
        public  string FirstNameEn { get; set; }
        public  string LastNameEn { get; set; }
        public  string FatherName { get; set; }
        public  string FatherNameEn { get; set; }//ToDo
        public  string HomePhone { get; set; }
        public  string HomePostCode { get; set; }
        public  string HomeAddress { get; set; }
        public  string PassportExpireDateDt { get; set; }
        public  string CertNo { get; set; }
        public int PersianCharRefId { get; set; }
        public string BirthCrtfctSeriesNumber { get; set; }
        public string BirthCrtfctSerial { get; set; }
    }

    public enum GenderType
    {
        Female = 1888,
        Male= 1889
    }
}