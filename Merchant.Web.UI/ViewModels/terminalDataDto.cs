namespace TES.Merchant.Web.UI.ViewModels
{
    public class terminalDataDto
    {
        public long Id { get; set; }
        public byte? PspId { get; set; }
        public string ChangeTopiarId { get; set; }
        public string ShebaNo { get; set; }
        public byte StatusId { get; set; }
        public string ContractNo { get; set; }
        public bool? NewParsian { get; set; }
        public string TerminalNo { get; set; }
        public int? TopiarId { get; set; }
    }
}