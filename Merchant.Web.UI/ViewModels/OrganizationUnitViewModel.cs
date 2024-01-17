namespace TES.Merchant.Web.UI.ViewModels
{
    public class OrganizationUnitViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public bool DisableNewTerminalRequest { get; set; }
        public bool DisableWirelessTerminalRequest { get; set; }
    }
}