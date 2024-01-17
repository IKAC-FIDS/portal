using System;
using System.Collections.Generic;

namespace TES.Merchant.Web.UI.ViewModels
{
    public class MessageDetailsViewModel
    {
        public string ExtraDataTopic;

        public long Id { get; set; }
        public long UserId { get; set; }
        public long? ReviewerUserId { get; set; }
        public DateTime CreationDate { get; set; }
        public string UserFullName { get; set; }
        public string ReviewerUserFullName { get; set; }
        public  long OrganizationUnitId { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public byte StatusId { get; set; }
        public DateTime? LastChangeStatusDate { get; set; }
        public Dictionary<long, string> Documents { get; set; }

        public ICollection<MessageReplyViewModel> Replies { get; set; }
        public string GUID { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public string ExtraDataTopicValue { get; set; }
        public string SerialNumber { get; set; }
        public int DamageValue { get; set; }
        public string SecondSubject { get; set; }
    }
}