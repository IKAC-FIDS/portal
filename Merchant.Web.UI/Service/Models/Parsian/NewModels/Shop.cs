namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class Shop
    {
        public  string ShopName { get; set; }
        public  string ShopNameEng { get; set; }
        public  int ShopSubMccRefId { get; set; }
        public int ShopCityRefId { get; set; }
        public ShopRegion  ShopRegionRefId { get; set; }
        public string ShopPostalCode { get; set; }
        public string ShopAddress { get; set; }
        public  string ShopPhone { get; set; }

        public string ShopMobNo { get; set; }   
        
        public  string ShopEmailAddress { get; set; } 
        public string WebAddress { get; set; }
        public string WebIp { get; set; }
        public  string WebPort { get; set; }
        public  string WebNamadType { get; set; }
        public  string WebNamadRegDateDt { get; set; }
        public  string WebNamadExpDateDt { get; set; }
        public  string TaxPayerCode { get; set; }
        
    }

    public enum ShopRegion
    {
        one = 1,
        two,
        three,
        foure,
        five,
        six,
        seven,
        eight,
        nine,
        ten,
        Eleven,
        Twelve,
        thirteen,
        chahardah,
        ponzdah,
        shoonzdah,
        efdah,
        egdah,
        noozdah,
        bist,
        bistoyek,
        bistodo,

    }
}