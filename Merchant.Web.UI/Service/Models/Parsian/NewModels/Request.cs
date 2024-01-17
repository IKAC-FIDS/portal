namespace TES.Merchant.Web.UI.Service.Models.Parsian.NewModels
{
    public class Request
    {
        public int ContractRefId { get; set; } = 1133573;  
        public int TerminalCount { get; set; }
        public TermTypeRefId TermTypeRefId { get; set; }
        public TerminalModel RequestTerminalModelRefId { get; set; }
        public PersonType PersonTypeRefId { get; set; }
        public DeviceModel RequestDeviceModelRefId { get; set; } // ToDo
    }

    public enum DeviceModel
    {
        Dialup = 31369,//ثابت رومیزی
        MobilePose = 31428,//mpos
        GRPRS = 31370 // GPRS
        
    }

    public enum PersonType
    {
        haghighiIrani = 31220,
        hoghoghiIrani = 31221,
        haghighiKhareji = 31222,
        hoghoghiKharegji = 31223
    }
    public enum TermTypeRefId
    {
        Physical = 12187,
        VirtualType = 31224
    }

    public enum TerminalModel
    {
        Dialup = 31369,
        GRPRS = 31370,
        internet = 31371,
        QR = 31405,
        MPL = 31407,
        IVR = 31408,
        Mobile = 31425,
        MobilePose = 31428,
        MiniCms = 1140922,
        sticker  = 1153507
    }
}