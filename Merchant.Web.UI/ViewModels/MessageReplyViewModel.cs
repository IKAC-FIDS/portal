using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MessageReplyViewModel
    {
        public long Id { get; set; }
        public long MessageId { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserFullName { get; set; }
        public bool IsCreator { get; set; }
        public Dictionary<long, string> Documents { get; set; }
    }
}