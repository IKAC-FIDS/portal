using System.Collections.Generic;
using System.Web;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class CreateMessageReplyViewModel
    {
        public long MessageId { get; set; }
        public string Body { get; set; }
        public List<HttpPostedFileBase> PostedFiles { get; set; }
    }
}