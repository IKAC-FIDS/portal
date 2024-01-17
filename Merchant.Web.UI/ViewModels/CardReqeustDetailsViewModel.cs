using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class CardReqeustDetailsViewModel
    {
        
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? ReviewerUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserFullName { get; set; }
        public string ReviewerUserFullName { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public byte StatusId { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
        public Dictionary<long, string> Documents { get; set; }

        public ICollection<MessageReplyViewModel> Replies { get; set; }
        public string GUID { get; set; }
        public string Status { get; set; }
        public string Price { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public string CardServiceType { get; set; }
        public string Priority { get; set; }
        public string TemplateId { get; set; }
        public string Branch { get; set; }
        public string PrintType { get; set; }
        public string DeliveryType { get; set; }
        public  string HasPacket { get; set; }
        public string EndDatre { get; set; }
        public string TemplateCode { get; set; }
        public object ByteArray { get; set; }
    }
}