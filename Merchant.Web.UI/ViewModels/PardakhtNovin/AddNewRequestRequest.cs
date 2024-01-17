using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels.PardakhtNovin
{
    public class AddNewRequestRequest
    {
        public AddNewRequestRequest()
        {
           // Childs = new List<AddNewRequestRequestDataDocs>();
        }
     //   public List<AddNewRequestRequestDataDocs> Childs { get; set; }
        public AddNewRequestRequestData Data { get; set; }
    }
    
    public class AddNewRequestRequestWithDocs
    {
        public AddNewRequestRequestWithDocs()
        {
            Childs = new List<AddNewRequestRequestDataDocs>();
        }
       public List<AddNewRequestRequestDataDocs> Childs { get; set; }
        public AddNewRequestRequestData Data { get; set; }
    }
}