using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewRequestRequestDataDocs
    {
        public string ChildName { get; set; } = "RequestMerchantDocument";
        public List<Document> Data { get; set; }
    }
}