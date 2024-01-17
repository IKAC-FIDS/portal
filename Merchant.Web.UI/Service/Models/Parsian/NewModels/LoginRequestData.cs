namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public  class LoginRequestData{
        public  string UserName { get; set; }
        public  string Secret { get; set; }
        public int TokenDuration { get; set; }
    }
}