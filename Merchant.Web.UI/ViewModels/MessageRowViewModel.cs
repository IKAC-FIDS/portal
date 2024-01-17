using System;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MessageRowViewModel
    {
        public long Id { get; set; }
        public byte StatusId { get; set; }
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string Subject { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
    }
}