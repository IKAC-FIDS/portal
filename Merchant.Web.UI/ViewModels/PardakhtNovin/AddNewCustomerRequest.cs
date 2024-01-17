using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewCustomerRequest
    {
      
        public AddNewCustomerRequestData Data { get; set; }
        public List<AddNewCustomerRequestDocs> Childs { get; set; }

    }
}