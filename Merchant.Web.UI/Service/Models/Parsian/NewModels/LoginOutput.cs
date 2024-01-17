namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class LoginOutput : OutPutError
    {
        public string LoginToken { get; set; }
        public  string ExpireTime { get; set; }
        public  string Status { get; set; }
        public  string Desc { get; set; }
        public  string StatusDesc { get; set; }
    }
}