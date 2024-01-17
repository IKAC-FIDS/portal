using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class OrganizationUnitGroupChangePermissionsViewModel
    {
        public List<long> OrganizationUnitIdList { get; set; }
        public bool DisableNewTerminalRequest { get; set; }
        public bool DisableWirelessTerminalRequest { get; set; }
    }
}