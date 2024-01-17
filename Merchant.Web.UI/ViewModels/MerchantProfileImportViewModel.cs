using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MerchantProfileImportViewModel
    {
        public IEnumerable<string> TerminalStatusList { get; set; }
        public IEnumerable<string> MarketerList { get; set; }
        public IEnumerable<string> NationalityList { get; set; }
        public IEnumerable<string> DeviceTypeList { get; set; }
        public IEnumerable<string> ActivityTypeList { get; set; }
        public IEnumerable<string> PspList { get; set; }
        public IEnumerable<string> BlockDocumentStatusList { get; set; }
    }
}