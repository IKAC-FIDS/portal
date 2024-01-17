namespace TES.Merchant.Web.UI.ViewModels
{
    public class PspAgentViewModel
    {
        public long Id { get; set; }
        public byte PspId { get; set; }
        public string Title { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string EmergencyTel { get; set; }
    }
}