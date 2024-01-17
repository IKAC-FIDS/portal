namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class LoginInput : ParsianInput
    {
        public LoginInput()
        {
            RequestData = new LoginRequestData();
        }
        public  LoginRequestData RequestData { get; set; }
       
    }
}